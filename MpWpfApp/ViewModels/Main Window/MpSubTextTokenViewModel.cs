using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSubTextTokenViewModel : MpViewModelBase {
        #region Properties
        private int _subTextTokenId = 0;
        public int SubTextTokenId {
            get {
                return _subTextTokenId;
            }
            set {
                if (_subTextTokenId != value) {
                    _subTextTokenId = value;
                    OnPropertyChanged(nameof(SubTextTokenId));
                }
            }
        }

        private int _copyItemId = 0;
        public int CopyItemId {
            get {
                return _copyItemId;
            }
            set {
                if (_copyItemId != value) {
                    _copyItemId = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        private string _tokenText = string.Empty;
        public string TokenText {
            get {
                return _tokenText;
            }
            set {
                if (_tokenText != value) {
                    _tokenText = value;
                    OnPropertyChanged(nameof(TokenText));
                }
            }
        }

        private MpSubTextTokenType _tokenType = MpSubTextTokenType.None;
        public MpSubTextTokenType TokenType {
            get {
                return _tokenType;
            }
            set {
                if (_tokenType != value) {
                    _tokenType = value;
                    OnPropertyChanged(nameof(TokenType));
                }
            }
        }

        private int _startIdx = 0;
        public int StartIdx {
            get {
                return _startIdx;
            }
            set {
                if (_startIdx != value) {
                    _startIdx = value;
                    OnPropertyChanged(nameof(StartIdx));
                }
            }
        }

        private int _endIdx = 0;
        public int EndIdx {
            get {
                return _endIdx;
            }
            set {
                if (_endIdx != value) {
                    _endIdx = value;
                    OnPropertyChanged(nameof(EndIdx));
                }
            }
        }

        private int _blockIdx = 0;
        public int BlockIdx {
            get {
                return _blockIdx;
            }
            set {
                if (_blockIdx != value) {
                    _blockIdx = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        private int _inlineIdx = 0;
        public int InlineIdx {
            get {
                return _inlineIdx;
            }
            set {
                if (_inlineIdx != value) {
                    _inlineIdx = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        private MpSubTextToken _subTextToken = null;
        public MpSubTextToken SubTextToken {
            get {
                return _subTextToken;
            }
            set {
                if (_subTextToken != value) {
                    _subTextToken = value;
                    OnPropertyChanged(nameof(SubTextToken));
                }
            }
        }

        public MpSubTextTokenViewModel(MpSubTextToken stt) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SubTextToken):
                        SubTextTokenId = SubTextToken.SubTextTokenId;
                        CopyItemId = SubTextToken.CopyItemId;
                        TokenText = SubTextToken.TokenText;
                        TokenType = SubTextToken.TokenType;
                        StartIdx = SubTextToken.StartIdx;
                        EndIdx = SubTextToken.EndIdx;
                        BlockIdx = SubTextToken.BlockIdx;
                        InlineIdx = SubTextToken.InlineIdx;
                        break;
                    case nameof(SubTextTokenId):
                        SubTextToken.SubTextTokenId = SubTextTokenId;
                        break;
                    case nameof(CopyItemId):
                        SubTextToken.CopyItemId = CopyItemId;
                        break;
                    case nameof(TokenText):
                        SubTextToken.TokenText = TokenText;
                        break;
                    case nameof(TokenType):
                        SubTextToken.TokenType = TokenType;
                        break;
                    case nameof(StartIdx):
                        SubTextToken.StartIdx = StartIdx;
                        break;
                    case nameof(EndIdx):
                        SubTextToken.EndIdx = EndIdx;
                        break;
                    case nameof(BlockIdx):
                        SubTextToken.BlockIdx = BlockIdx;
                        break;
                    case nameof(InlineIdx):
                        SubTextToken.InlineIdx = InlineIdx;
                        break;
                }
            };

            SubTextToken = stt;
        }
        #endregion
    }
}
