namespace MonkeyPaste.Common.Plugin {
    public interface MpIAnnotation {

        string type { get; set; }
        //double score { get; set; }
        //string label { get; set; }
        //string body { get; set; }
    }
    public interface MpIContentElement : MpIStyleProperties {
        string type { get; set; }
        string content { get; set; }
    }
    public interface MpIDocumentElement {
        MpIContentElement header { get; }
        MpIContentElement body { get; }
        MpIContentElement footer { get; }

    }
    public interface MpIAnnotationNode : MpITreeNode, MpIAnnotation {
    }
    public interface MpITextRange {
        int Offset { get; }
        int Length { get; }
    }
}
