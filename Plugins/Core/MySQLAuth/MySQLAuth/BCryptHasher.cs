/*
        Copyright (c) 2025, pGina MySQL 8 Team
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
using log4net;

namespace pGina.Plugin.MySQLAuth
{
    /// <summary>
    /// Provides BCrypt password hashing functionality.
    /// BCrypt is the recommended algorithm for password storage.
    /// </summary>
    public static class BCryptHasher
    {
        private static readonly ILog m_logger = LogManager.GetLogger("BCryptHasher");

        public const int MinWorkFactor = 4;
        public const int MaxWorkFactor = 12;
        public const int DefaultWorkFactor = 10;

        public static string HashPassword(string plainTextPassword, int workFactor = DefaultWorkFactor)
        {
            if (plainTextPassword == null)
                throw new ArgumentNullException("plainTextPassword");

            if (workFactor < MinWorkFactor || workFactor > MaxWorkFactor)
                workFactor = DefaultWorkFactor;

            try
            {
                return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor);
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("BCrypt hash error: {0}", ex.Message);
                throw;
            }
        }

        public static bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            if (plainTextPassword == null || hashedPassword == null)
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("BCrypt verify error: {0}", ex.Message);
                return false;
            }
        }

        public static bool IsBCryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            return hash.Length == 60 && 
                   (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));
        }

        public static int GetWorkFactor(string hashedPassword)
        {
            if (!IsBCryptHash(hashedPassword))
                return -1;

            try
            {
                return int.Parse(hashedPassword.Substring(4, 2));
            }
            catch
            {
                return -1;
            }
        }
    }
}
