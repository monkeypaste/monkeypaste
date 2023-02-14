namespace MonkeyPaste {
    public interface MpIBindingContext<VM>
        where VM : class {

        //FE ThisFrameworkElement { get; }

        VM BindingContext { get; set; }

        VM GetBindingContext();

        void SetBindingContext(VM vm);
    }
}
