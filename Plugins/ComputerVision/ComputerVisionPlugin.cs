using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using MonkeyPaste.Common;

namespace ComputerVision {
    public class ComputerVisionPlugin : MpIAnalyzeAsyncComponent {
        // Add your Computer Vision subscription key and endpoint
        static string subscriptionKey = "b455280a2c66456e926b66a1e6656ce3";
        static string endpoint = "https://mp-azure-cognitive-services-resource-instance.cognitiveservices.azure.com/";

         public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            string url_query = string.Empty;
            var features = req.GetRequestParamStringListValue(2);
            if(features.Count > 0) {
                url_query = $"visualFeatures={string.Join(",", features)}";
            }

            var details = req.GetRequestParamStringListValue(3);
            if(details .Count > 0) {
                if(!string.IsNullOrEmpty(url_query)) {
                    url_query += "&";
                }
                url_query += $"details={string.Join(",",details)}";
            }

            string base64ImgStr = req.GetRequestParamStringValue(4);

            if(string.IsNullOrEmpty(url_query) || string.IsNullOrEmpty(base64ImgStr)) {
                // no options specified 
                return null;
            }

            var resp = new MpAnalyzerPluginResponseFormat();
            string endpoint_url = $"{endpoint}vision/v3.2/analyze?{url_query}";
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(
                    new HttpMethod("POST"), endpoint_url)) {
                    request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);

                    request.Content = new ByteArrayContent(Convert.FromBase64String(base64ImgStr));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    try {
                        var http_response = await httpClient.SendAsync(request);
                        string http_response_str = await http_response.Content.ReadAsStringAsync();
                        resp = CreateAnnotations(resp, http_response_str);

                    }
                    catch(Exception ex) {
                        resp.errorMessage = ex.Message;
                    }
                    
                }
            }
            return resp;
        }

        MpAnalyzerPluginResponseFormat CreateAnnotations(MpAnalyzerPluginResponseFormat resp, string respJsonStr) {
            Root root = JsonConvert.DeserializeObject<Root>(respJsonStr);
            List<MpAnnotationNodeFormat> annotations = new List<MpAnnotationNodeFormat>();

            if(root.categories != null && root.categories.Count > 0) {
                root.categories.Select(x => ProcessCategory(x)).Where(x => x != null).ForEach(x => annotations.Add(x));
            }
            if(root.tags != null && root.tags.Count > 0) {
                root.tags.Select(x => ProcessTag(x)).Where(x => x != null).ForEach(x => annotations.Add(x));
            }
            if (root.objects != null && root.objects.Count > 0) {
                root.objects.Select(x => ProcessObject(x)).Where(x => x != null).ForEach(x => annotations.Add(x));
            }

            if(annotations.Count > 0) {
                var root_annotation = new MpAnnotationNodeFormat() {
                    children = annotations
                };
                resp.dataObject =
                    new MpPortableDataObject(
                        MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT,
                        root_annotation.SerializeJsonObject());
            }



            return resp;
        }


        MpAnnotationNodeFormat ProcessObject(VisionObject vObject) {
            if(vObject == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Object",
                label = vObject.@object,
                score = vObject.confidence,
                children = ProcessRectangle(vObject.rectangle,"Object") is MpAnnotationNodeFormat da ? new List<MpAnnotationNodeFormat>() { da } : null
            };
        }
        MpAnnotationNodeFormat ProcessTag(Tag tag) {
            if(tag == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Tag",
                label = tag.name,
                score = tag.confidence
            };
        }
        MpAnnotationNodeFormat ProcessCategory(Category category) {
            if(category == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Category",
                label = category.name,
                score = category.score,
                children = ProcessDetail(category.detail) is MpAnnotationNodeFormat da ? new List<MpAnnotationNodeFormat>() { da} : null
            };
        }
        MpAnnotationNodeFormat ProcessDetail(Detail detail) {
            if(detail == null) {
                return null;
            }
            List<MpAnnotationNodeFormat> children = new List<MpAnnotationNodeFormat>();
            if(detail.celebrities != null && 
                detail.celebrities.Count > 0) {
                if(detail.celebrities.Select(x=>ProcessCelebrity(x)) is IEnumerable<MpAnnotationNodeFormat> al &&
                    al.Where(x=>x != null).Count() > 0) {
                    children.AddRange(al.Where(x=>x != null));
                }
            }
            if (detail.landmarks != null &&
                detail.landmarks.Count > 0) {
                if (detail.landmarks.Select(x => ProcessLandmark(x)) is IEnumerable<MpAnnotationNodeFormat> al &&
                    al.Where(x => x != null).Count() > 0) {
                    children.AddRange(al.Where(x => x != null));
                }
            }
            if (children.Count == 0) {
                return null;
            }

            return new MpAnnotationNodeFormat() {
                type = "Detail",
                children = children
            };
        }

        MpAnnotationNodeFormat ProcessCelebrity(Celebrity celeb) {
            if(celeb == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Celebrity",
                label = celeb.name,
                score = celeb.confidence,
                children = celeb.faceRectangle == null ? null : new List<MpAnnotationNodeFormat>() { ProcessFaceRectangle(celeb.faceRectangle) }
            };
        }
        
        MpAnnotationNodeFormat ProcessLandmark(Landmark landmark) {
            if(landmark == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Landmark",
                label = landmark.name,
                score = landmark.confidence
            };
        }

        MpAnnotationNodeFormat ProcessFaceRectangle(FaceRectangle fr) {
            if(fr == null) {
                return null;
            }
            return new MpImageAnnotationNodeFormat() {
                type = "FaceRectangle",
                left = fr.left,
                top = fr.top,
                right = fr.left + fr.width,
                bottom = fr.top + fr.height
            };
        }
        
        MpAnnotationNodeFormat ProcessRectangle(Rectangle rect, string rectType) {
            if(rect == null) {
                return null;
            }
            return new MpImageAnnotationNodeFormat() {
                type = rectType,
                left = rect.x,
                top = rect.y,
                right = rect.x + rect.w,
                bottom = rect.y + rect.h
            };
        }
    }
}
