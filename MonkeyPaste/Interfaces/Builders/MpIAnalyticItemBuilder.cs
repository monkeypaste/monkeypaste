﻿using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIAnalyticItemBuilder {
        Task<MpAnalyticItem> Create(
            string endPoint,
            string apiKey,
            MpInputFormatType format,
            string title,
            string description,
            MpIIconBuilder iconBuilder);
    }
}