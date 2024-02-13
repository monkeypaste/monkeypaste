
// import ExecutionEnvironment from '@docusaurus/ExecutionEnvironment';
// function googleTranslateElementInit() {
//     new google.translate.TranslateElement(
//         {
//             pageLanguage: 'en',
//             layout: google.translate.TranslateElement.InlineLayout.SIMPLE
//         },
//         'google_translate_element');
// }

// if (ExecutionEnvironment.canUseDOM) {
window.onload = function () {
    document.querySelector('[href="#translate-btn"]').addEventListener('click', () => {
        console.log('Translate button clicked.');
        new google.translate.TranslateElement(
            {
                pageLanguage: 'en',
                layout: google.translate.TranslateElement.InlineLayout.SIMPLE
            },
            'google_translate_element');
    });
};
//}