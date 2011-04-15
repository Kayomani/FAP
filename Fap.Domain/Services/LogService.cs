using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Targets;
using NLog.Config;
using NLog;
using System.Diagnostics;
using NLog.Targets.Wrappers;

namespace Fap.Domain.Services
{
    public class LogService
    {
        private MemoryTarget mtarget;
        private LoggingRule rule;
        private int readLine;
        private bool moreDebug = false;

        public LogService()
        {
            LoggingConfiguration config = LogManager.Configuration;
            mtarget = new MemoryTarget();
            mtarget.Layout = "${logger} -  ${message}";
            mtarget.Name = "Memory";
            AsyncTargetWrapper wrapper = new AsyncTargetWrapper(mtarget);

            config.AddTarget("Memory", wrapper);
           

            rule = new LoggingRule("*", LogLevel.Info, mtarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;

            if (Debugger.IsAttached)
                moreDebug = true;
        }

        public bool MoreDebug
        {
            set
            {
                moreDebug = value;
              /*  This doesnt work??
               * rule.EnableLoggingForLevel(LogLevel.Trace);
                rule.EnableLoggingForLevel(LogLevel.Debug);
                rule.EnableLoggingForLevel(LogLevel.Error);
                rule.EnableLoggingForLevel(LogLevel.Fatal);
                rule.EnableLoggingForLevel(LogLevel.Info);
                rule.EnableLoggingForLevel(LogLevel.Trace);
                rule.EnableLoggingForLevel(LogLevel.Warn);
                LogManager.Configuration.LoggingRules.Remove(rule);*/
            }
            get { return moreDebug; }
        }

        public List<string> GetLines()
        {
            List<string> lines = new List<string>();
            if (null != mtarget && moreDebug)
            {
                for (int i = readLine; i < mtarget.Logs.Count; i++)
                    lines.Add(mtarget.Logs[i]);
                readLine = mtarget.Logs.Count;
            }
            return lines;
        }
    }
}
