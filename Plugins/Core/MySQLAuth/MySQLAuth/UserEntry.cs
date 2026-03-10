/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using log4net;

namespace pGina.Plugin.MySQLAuth
{
    /// <summary>
    /// Supported password hash algorithms.
    /// BCRYPT is the recommended algorithm for new implementations (Phase 1).
    /// </summary>
    public enum PasswordHashAlgorithm
    {
        NONE,       // Plain text (not recommended)
        MD5,        // Legacy - not recommended for new passwords
        SHA1,       // Legacy - not recommended for new passwords
        SHA256,     // Acceptable but BCRYPT preferred
        SHA384,     // Acceptable but BCRYPT preferred
        SHA512,     // Acceptable but BCRYPT preferred
        SMD5,       // Salted MD5 - legacy
        SSHA1,      // Salted SHA1 - legacy
        SSHA256,    // Salted SHA256
        SSHA384,    // Salted SHA384
        SSHA512,    // Salted SHA512
        BCRYPT      // Recommended - modern secure hashing (Phase 1)
    }

    /// <summary>
    /// Represents a user entry from the MySQL database with password hash information.
    /// Handles password verification for multiple hash algorithms including BCrypt.
    /// </summary>
    class UserEntry
    {
        private ILog m_logger = LogManager.GetLogger("MySQLAuth.UserEntry");

        private string m_hashedPass;
        private PasswordHashAlgorithm m_hashAlg;
        private string m_name;
        private byte[] m_passBytes;

        public string Name { get { return m_name; } }
        public PasswordHashAlgorithm HashAlg { get { return m_hashAlg; } }
        private string HashedPassword { get { return m_hashedPass; } }

        public UserEntry(string uname, PasswordHashAlgorithm alg, string hashedPass)
        {
            m_name = uname;
            m_hashAlg = alg;
            m_hashedPass = hashedPass;

            // For BCrypt and NONE, we keep the hash as string (not converted to bytes)
            // For other algorithms, we decode to bytes for comparison
            if (m_hashAlg != PasswordHashAlgorithm.NONE && m_hashAlg != PasswordHashAlgorithm.BCRYPT)
            {
                m_passBytes = this.Decode(m_hashedPass);
            }
            else
            {
                m_passBytes = null;
            }
        }

        private byte[] Decode(string hash)
        {
            int encInt = Settings.Store.HashEncoding;
            Settings.HashEncoding encoding = (Settings.HashEncoding)encInt;
            
            if (encoding == Settings.HashEncoding.HEX)
                return FromHexString(hash);
            else if (encoding == Settings.HashEncoding.BASE_64)
                return Convert.FromBase64String(hash);
            else
            {
                m_logger.ErrorFormat("Unrecognized hash encoding!");
                throw new Exception("Unrecognized hash encoding.");
            }
        }

        /// <summary>
        /// Verifies a plain text password against the stored hash.
        /// Supports BCrypt and legacy hash algorithms.
        /// </summary>
        public bool VerifyPassword(string plainText)
        {
            if (plainText == null)
                return false;

            // Handle BCrypt separately (Phase 1)
            if (HashAlg == PasswordHashAlgorithm.BCRYPT)
            {
                return VerifyBCryptPassword(plainText);
            }

            // If hash algorithm is NONE, just compare the strings
            if (HashAlg == PasswordHashAlgorithm.NONE)
                return plainText.Equals(HashedPassword);

            // Is it a salted hash?
            if (HashAlg == PasswordHashAlgorithm.SMD5 ||
                HashAlg == PasswordHashAlgorithm.SSHA1 ||
                HashAlg == PasswordHashAlgorithm.SSHA256 ||
                HashAlg == PasswordHashAlgorithm.SSHA384 ||
                HashAlg == PasswordHashAlgorithm.SSHA512)
            {
                return VerifySaltedPassword(plainText);
            }

            // Unsalted hash - hash and compare bytes
            byte[] hashedPlainText = HashPlainText(plainText);
            if (hashedPlainText != null)
                return hashedPlainText.SequenceEqual(m_passBytes);
            else
                return false;
        }

        /// <summary>
        /// Verifies a password using BCrypt algorithm.
        /// Uses BCrypt.Net library directly.
        /// </summary>
        private bool VerifyBCryptPassword(string plainText)
        {
            try
            {
                // BCrypt.Net.BCrypt.Verify extracts salt from hash
                // and uses it to hash the input password for comparison
                bool result = BCrypt.Net.BCrypt.Verify(plainText, m_hashedPass);

                if (result)
                {
                    m_logger.InfoFormat("BCrypt authentication successful for user: {0}", m_name);
                }
                else
                {
                    m_logger.DebugFormat("BCrypt authentication failed for user: {0}", m_name);
                }

                return result;
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                m_logger.ErrorFormat("Invalid BCrypt hash format for user {0}: {1}", m_name, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("BCrypt verification error for user {0}: {1}", m_name, ex.Message);
                return false;
            }
        }

        private bool VerifySaltedPassword(string plainText)
        {
            using (HashAlgorithm hasher = GetHasher())
            {
                if (hasher != null)
                {
                    if (hasher.HashSize % 8 != 0)
                        m_logger.WarnFormat("Hash size is not a multiple of 8. Verification may be incorrect.");

                    int hashSizeBytes = hasher.HashSize / 8;

                    if (m_passBytes.Length > hashSizeBytes)
                    {
                        // Get the salt (bytes after the hash)
                        byte[] salt = new byte[m_passBytes.Length - hashSizeBytes];
                        Array.Copy(m_passBytes, hashSizeBytes, salt, 0, salt.Length);
                        m_logger.DebugFormat("Found {1} byte salt for user {0}", m_name, salt.Length);

                        // Get the stored hash
                        byte[] storedHash = new byte[hashSizeBytes];
                        Array.Copy(m_passBytes, 0, storedHash, 0, hashSizeBytes);

                        // Build array with plain text and salt
                        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                        byte[] plainTextAndSalt = new byte[plainTextBytes.Length + salt.Length];
                        plainTextBytes.CopyTo(plainTextAndSalt, 0);
                        salt.CopyTo(plainTextAndSalt, plainTextBytes.Length);

                        // Compute hash and compare
                        byte[] computedHash = hasher.ComputeHash(plainTextAndSalt);
                        return computedHash.SequenceEqual(storedHash);
                    }
                    else
                    {
                        m_logger.ErrorFormat("Hash too short for salted algorithm. Length: {0}, Expected > {1}",
                            m_passBytes.Length, hashSizeBytes);
                        return false;
                    }
                }
            }

            return false;
        }

        private byte[] HashPlainText(string plainText)
        {
            if (HashAlg == PasswordHashAlgorithm.NONE)
                throw new Exception("Tried to hash a password when algorithm is NONE.");

            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] result = null;

            using (HashAlgorithm hasher = GetHasher())
            {
                if (hasher != null)
                    result = hasher.ComputeHash(bytes);
            }

            return result;
        }

        private HashAlgorithm GetHasher()
        {
            switch (HashAlg)
            {
                case PasswordHashAlgorithm.NONE:
                case PasswordHashAlgorithm.BCRYPT:
                    return null;
                case PasswordHashAlgorithm.MD5:
                case PasswordHashAlgorithm.SMD5:
                    return MD5.Create();
                case PasswordHashAlgorithm.SHA1:
                case PasswordHashAlgorithm.SSHA1:
                    return SHA1.Create();
                case PasswordHashAlgorithm.SHA256:
                case PasswordHashAlgorithm.SSHA256:
                    return SHA256.Create();
                case PasswordHashAlgorithm.SHA512:
                case PasswordHashAlgorithm.SSHA512:
                    return SHA512.Create();
                case PasswordHashAlgorithm.SHA384:
                case PasswordHashAlgorithm.SSHA384:
                    return SHA384.Create();
                default:
                    m_logger.ErrorFormat("Unrecognized hash algorithm!");
                    return null;
            }
        }

        private byte[] FromHexString(string hex)
        {
            if (hex.Length % 2 != 0)
                hex = "0" + hex;

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Checks if a string appears to be a BCrypt hash.
        /// BCrypt hashes are 60 characters and start with $2a$, $2b$, or $2y$.
        /// </summary>
        public static bool IsBCryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            return hash.Length == 60 &&
                   (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));
        }
    }
}
