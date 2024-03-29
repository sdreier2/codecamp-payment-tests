﻿using System;

namespace Common
{
    public interface ILogger
    {
        void LogDebug(string message);
        void LogInfo(string message);

        void LogWarning(string message);

        void LogError(string message);

    }
}
