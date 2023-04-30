namespace MonkeyPaste {
    public enum MpRoutingType {
        None = 0,
        Internal, //1
        Bubble, //3 sendkey before
        Tunnel,  //4 sendkey after
        Override,
        Passive
    }

}
