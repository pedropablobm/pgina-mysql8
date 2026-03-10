/*
        Copyright (c) 2013, pGina Team
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

namespace pGina.Plugin.MySQLAuth
{
    /// <summary>
    /// Settings management class for MySQL Authentication plugin.
    /// Contains all configuration options including security settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Encoding format for password hashes in the database.
        /// </summary>
        public enum HashEncoding { HEX = 0, BASE_64 = 1 };

        private static dynamic m_settings = new pGina.Shared.Settings.pGinaDynamicSettings(PluginImpl.PluginUuid);
        
        /// <summary>
        /// Dynamic settings store for accessing configuration values.
        /// </summary>
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            // =============================================
            // Database Connection Settings
            // =============================================
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("UseSsl", false);
            m_settings.SetDefault("User", "pgina_user");
            m_settings.SetDefaultEncryptedSetting("Password", "secret");
            m_settings.SetDefault("Database", "account_db");

            // Connection timeout settings (for MySQL 8/MariaDB)
            m_settings.SetDefault("ConnectionTimeout", 30);
            m_settings.SetDefault("CommandTimeout", 30);

            // =============================================
            // User Table Configuration
            // =============================================
            m_settings.SetDefault("Table", "users");
            m_settings.SetDefault("HashEncoding", (int)HashEncoding.HEX);
            m_settings.SetDefault("UsernameColumn", "user_name");
            m_settings.SetDefault("HashMethodColumn", "hash_method");
            m_settings.SetDefault("PasswordColumn", "password");
            m_settings.SetDefault("UserTablePrimaryKeyColumn", "user_id");

            // =============================================
            // Group Table Configuration
            // =============================================
            m_settings.SetDefault("GroupTableName", "groups");
            m_settings.SetDefault("GroupNameColumn", "group_name");
            m_settings.SetDefault("GroupTablePrimaryKeyColumn", "group_id");

            // User-Group relationship table
            m_settings.SetDefault("UserGroupTableName", "usergroup");
            m_settings.SetDefault("UserForeignKeyColumn", "user_id");
            m_settings.SetDefault("GroupForeignKeyColumn", "group_id");

            // =============================================
            // Authorization Settings
            // =============================================
            m_settings.SetDefault("GroupAuthzRules", new string[] { (new GroupAuthzRule(true)).ToRegString() });
            m_settings.SetDefault("AuthzRequireMySqlAuth", false);

            // Gateway settings
            m_settings.SetDefault("GroupGatewayRules", new string[] { });
            m_settings.SetDefault("PreventLogonOnServerError", false);

            // =============================================
            // BCrypt Security Settings (Phase 1)
            // =============================================
            
            /// <summary>
            /// Work factor (cost) for BCrypt hashing.
            /// Range: 4-12. Higher values are more secure but slower.
            /// Recommended: 10-12 for production environments.
            /// Work factor 10 = ~100ms, 11 = ~200ms, 12 = ~400ms
            /// </summary>
            m_settings.SetDefault("BCryptWorkFactor", 10);
            
            /// <summary>
            /// Enable automatic migration of legacy hashes (MD5, SHA) to BCrypt.
            /// When enabled, after successful authentication with a legacy hash,
            /// the password will be rehashed using BCrypt and stored in the database.
            /// Requires the PasswordColumn to be writable.
            /// </summary>
            m_settings.SetDefault("MigrateToBCrypt", false);

            // =============================================
            // Account Lockout Settings (Phase 2 - Placeholder)
            // =============================================
            
            /// <summary>
            /// Enable account lockout after failed login attempts.
            /// </summary>
            m_settings.SetDefault("EnableAccountLockout", false);
            
            /// <summary>
            /// Number of failed attempts before account lockout.
            /// </summary>
            m_settings.SetDefault("MaxFailedAttempts", 5);
            
            /// <summary>
            /// Duration of account lockout in minutes.
            /// </summary>
            m_settings.SetDefault("LockoutDurationMinutes", 30);

            // =============================================
            // Audit Logging Settings (Phase 3 - Placeholder)
            // =============================================
            
            /// <summary>
            /// Enable authentication event logging to database.
            /// </summary>
            m_settings.SetDefault("EnableAuthLogging", false);
            
            /// <summary>
            /// Table name for authentication logs.
            /// </summary>
            m_settings.SetDefault("AuthLogTable", "auth_logs");
        }

        /// <summary>
        /// Gets the configured BCrypt work factor, ensuring it's within valid range (4-12).
        /// </summary>
        public static int GetBCryptWorkFactor()
        {
            int workFactor = m_settings.BCryptWorkFactor;
            
            // Ensure work factor is within valid range
            if (workFactor < 4 || workFactor > 12)
            {
                return 10; // Default safe value
            }
            
            return workFactor;
        }

        /// <summary>
        /// Checks if BCrypt hash migration is enabled.
        /// </summary>
        public static bool IsMigrationEnabled()
        {
            return m_settings.MigrateToBCrypt;
        }
    }
}
