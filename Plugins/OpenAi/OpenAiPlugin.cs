using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using OpenAi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAi {
    public class OpenAiPlugin : MpIAnalyzeComponentAsync, MpISupportHeadlessAnalyzerComponentFormat {
        const string PARAM_ID_MODEL = "model";
        const string PARAM_ID_ORG_ID = "orgid";
        const string PARAM_ID_API_KEY = "apikey";
        const string PARAM_ID_TEMPERATURE = "temp";
        const string PARAM_ID_CONTENT = "content";
        const string PARAM_ID_SIGNUP = "signup";

        const string END_POINT_URL = "https://api.openai.com/v1/chat/completions";

        const string SIGNUP_URL = "https://platform.openai.com/signup";
        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {


            var resp = new MpAnalyzerPluginResponseFormat();
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(
                    new HttpMethod("POST"), END_POINT_URL)) {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {req.GetRequestParamStringValue(PARAM_ID_API_KEY)}");
                    if (req.GetRequestParamStringValue(PARAM_ID_ORG_ID) is string org_id) {
                        request.Headers.TryAddWithoutValidation("OpenAI-Organization", org_id);
                    }
                    var opai_req = new OpenAiRequest() {
                        model = req.GetRequestParamStringValue(PARAM_ID_MODEL),
                        temperature = req.GetRequestParamDoubleValue(PARAM_ID_TEMPERATURE),
                        messages = new[] {
                            new OpenAiMessage() {
                                role = "user",
                                content = req.GetRequestParamStringValue(PARAM_ID_CONTENT)
                            }
                        }.ToList()
                    };
                    request.Content = new StringContent(JsonSerializer.Serialize(opai_req));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    try {
                        var http_response = await httpClient.SendAsync(request);
                        string http_response_str = await http_response.Content.ReadAsStringAsync();
                        if (http_response.IsSuccessStatusCode) {
                            var opai_resp = JsonSerializer.Deserialize<OpenAiResponse>(http_response_str);
                            if (opai_resp.choices != null &&
                                opai_resp.choices.FirstOrDefault(x => x.message != null) is { } msg_choice) {
                                resp.dataObjectLookup = new Dictionary<string, object> {
                                    {MpPortableDataFormats.Text, msg_choice.message.content}
                                };
                            }
                        } else {
                            // invalidate creds
                            resp.invalidParams.Add(PARAM_ID_API_KEY, http_response_str);
                        }
                    }

                    catch (Exception ex) {
                        resp.errorMessage = ex.Message;
                    }

                }
            }
            return resp;
        }
        public MpAnalyzerPluginFormat GetFormat(MpHeadlessAnalyzerComponentFormatRequest request) {
            return new MpAnalyzerPluginFormat() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                outputType = new MpPluginOutputFormat() {
                    text = true
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = "Model",
                        isRequired = true,
                        controlType = MpParameterControlType.ComboBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        values = new[] {
                            "gpt-4",
                            "gpt-4-32k",
                            "gpt-3.5-turbo",
                            "gpt-3.5-turbo-16k",
                            "dall-e-3"
                        }
                        .Select((x,idx)=>new MpPluginParameterValueFormat(x,idx == 0)).ToList(),
                        paramId = PARAM_ID_MODEL,
                    },
                    new MpParameterFormat() {
                        label = "Temperature",
                        description = "High values will create more random or creative responses. Low values are more factual and focused.",
                        controlType = MpParameterControlType.Slider,
                        unitType = MpParameterValueUnitType.Decimal,
                        isRequired = true,
                        value = new MpPluginParameterValueFormat(0.75.ToString(),true),
                        paramId = PARAM_ID_TEMPERATURE,
                    },
                    new MpParameterFormat() {
                        isVisible = true,
                        isRequired = true,
                        label = "Prompt",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        value = new MpPluginParameterValueFormat("{ItemData}",true),
                        paramId = PARAM_ID_CONTENT,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        isSharedValue = true,
                        isRequired = false,
                        label = "Organization (optional)",
                        description = "Usage will be applied to the provided organization",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = PARAM_ID_ORG_ID,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        isSharedValue = true,
                        isRequired = true,
                        label = "API Key",
                        description = "Click the link below to signup for a free api key",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = PARAM_ID_API_KEY,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        controlType = MpParameterControlType.Hyperlink,
                        value = new MpPluginParameterValueFormat(SIGNUP_URL,"Sign Up",true),
                        paramId = PARAM_ID_SIGNUP,
                    },
                }
            };
        }
    }
}
