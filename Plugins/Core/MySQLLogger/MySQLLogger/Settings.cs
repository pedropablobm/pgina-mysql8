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
using System.IO;

using pGina.Shared.Settings;

namespace pGina.Plugin.MySqlLogger
{
    

    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            // Set defaults
            //m_settings.SetDefault("LoggerMode", LoggerMode.EVENT);
            m_settings.SetDefault("EventMode", true);
            m_settings.SetDefault("SessionMode", false);
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("User", "pGina");
            m_settings.SetDefaultEncryptedSetting("Password", "secret", null);
            m_settings.SetDefault("Database", "pGinaDB");
            m_settings.SetDefault("SessionTable", "");
            m_settings.SetDefault("EventTable", "pGinaLog");

            m_settings.SetDefault("EvtLogon", true);
            m_settings.SetDefault("EvtLogoff", true);
            m_settings.SetDefault("EvtLock", false);
            m_settings.SetDefault("EvtUnlock", false);
            m_settings.SetDefault("EvtConsoleConnect",false);
            m_settings.SetDefault("EvtConsoleDisconnect", false);
            m_settings.SetDefault("EvtRemoteControl", false);
            m_settings.SetDefault("EvtRemoteConnect", false);
            m_settings.SetDefault("EvtRemoteDisconnect", false);

            m_settings.SetDefault("UseModifiedName", false);
            m_settings.SetDefault("OfflineQueueEnabled", true);
            m_settings.SetDefault("HealthCheckSeconds", 30);
            m_settings.SetDefault("FlushBatchSize", 100);
            m_settings.SetDefault("OfflineQueuePath", string.Empty);
        }

        public static uint GetPort()
        {
            return Convert.ToUInt32(m_settings.Port);
        }

        public static bool GetEventMode()
        {
            return Convert.ToBoolean(m_settings.EventMode);
        }

        public static bool GetSessionMode()
        {
            return Convert.ToBoolean(m_settings.SessionMode);
        }

        public static bool GetUseModifiedName()
        {
            return Convert.ToBoolean(m_settings.UseModifiedName);
        }

        public static bool GetEvtLogon()
        {
            return Convert.ToBoolean(m_settings.EvtLogon);
        }

        public static bool GetEvtLogoff()
        {
            return Convert.ToBoolean(m_settings.EvtLogoff);
        }

        public static bool GetEvtLock()
        {
            return Convert.ToBoolean(m_settings.EvtLock);
        }

        public static bool GetEvtUnlock()
        {
            return Convert.ToBoolean(m_settings.EvtUnlock);
        }

        public static bool GetEvtConsoleConnect()
        {
            return Convert.ToBoolean(m_settings.EvtConsoleConnect);
        }

        public static bool GetEvtConsoleDisconnect()
        {
            return Convert.ToBoolean(m_settings.EvtConsoleDisconnect);
        }

        public static bool GetEvtRemoteControl()
        {
            return Convert.ToBoolean(m_settings.EvtRemoteControl);
        }

        public static bool GetEvtRemoteConnect()
        {
            return Convert.ToBoolean(m_settings.EvtRemoteConnect);
        }

        public static bool GetEvtRemoteDisconnect()
        {
            return Convert.ToBoolean(m_settings.EvtRemoteDisconnect);
        }

        public static bool IsOfflineQueueEnabled()
        {
            return Convert.ToBoolean(m_settings.OfflineQueueEnabled);
        }

        public static int GetHealthCheckSeconds()
        {
            return Math.Max(5, Convert.ToInt32(m_settings.HealthCheckSeconds));
        }

        public static int GetFlushBatchSize()
        {
            return Math.Max(1, Convert.ToInt32(m_settings.FlushBatchSize));
        }

        public static string GetOfflineQueuePath()
        {
            string configuredPath = Convert.ToString(m_settings.OfflineQueuePath);
            if (!string.IsNullOrWhiteSpace(configuredPath))
                return configuredPath;

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "pGina",
                "MySQLLogger",
                "mysqllogger-queue.sqlite");
        }

    }
}
