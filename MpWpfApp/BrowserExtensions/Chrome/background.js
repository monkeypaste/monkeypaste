chrome.runtime.onInstalled.addListener(function () {
    chrome.storage.sync.set({ color: '#3aa757' }, function () {
        console.log('The color is green.');
    });
    chrome.declarativeContent.onPageChanged.removeRules(undefined, function () {
        chrome.declarativeContent.onPageChanged.addRules([{
            conditions: [new chrome.declarativeContent.PageStateMatcher({
                pageUrl: { hostEquals: 'developer.chrome.com' },
            })
            ],
            actions: [new chrome.declarativeContent.ShowPageAction()]
        }]);
    });
    //chrome.tabs.query({ active: true, lastFocusedWindow: true }, tabs => {
    //    let url = tabs[0].url;
    //    // use `url` here inside the callback because it's asynchronous!
    //});  
});
chrome.tabs.onActivated.addListener(function(activeInfo) {
    sendCurrentUrl();
});
chrome.tabs.onSelectionChanged.addListener(function() {
    sendCurrentUrl();
});
function sendCurrentUrl() {
    chrome.tabs.getSelected(null, function (tab) {
        var tablink = tab.url
        console.log(tablink);
    });
}