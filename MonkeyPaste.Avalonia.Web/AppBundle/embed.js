import { dotnetRuntime } from './main.js';

function createIframe(hostGuid, srcUrl) {
    var iframe_elm = globalThis.document.createElement('iframe');

    iframe_elm.addEventListener('load', onIframeLoaded);

    iframe_elm.id = hostGuid;
    //iframe_elm.setAttribute('src', 'Editor/index.html?auto_test');
    iframe_elm.setAttribute('src', srcUrl);
    iframe_elm.setAttribute('crossorigin', true);
    iframe_elm.setAttribute('credentialless', true);

    return iframe_elm;
}
function onIframeLoaded(e) {
    onIframeNavigated(e.srcElement.id, e.srcElement.getAttribute('src'));
}

function navigateIframe(iframe_elm, url) {
    //let iframe_elm = getEditorIframeElementByHostGuid(hostGuid);
    if (!iframe_elm) {
        console.log('no iframe found for guid: ' + hostGuid + ' cannot set url ' + url);
        return;
    }
    iframe_elm.setAttribute('src', url);
}


function getEditorIframeElementByHostGuid(hostGuid) {
    return globalThis.document.getElementById(hostGuid);
}

function sendMessageToIframe(editor_elm, msg) {
    if (!msg) {
        return;
    }
    //let editor_elm = getEditorIframeElementByHostGuid(hostGuid);
    if (!editor_elm) {
        return;
    }
    editor_elm.contentWindow.postMessage(msg);
    console.log('msg to editor: ' + msg)
}

window.addEventListener('message', onReceivedIframeMessage, false);

function onReceivedIframeMessage(e) {
    console.log('recevd iframe message: ' + e);
    sendMessageToHost(e.guid, e.type, e.data);
}

export async function onIframeNavigated(hostGuid, url) {
    const { getAssemblyExports } = dotnetRuntime;//await getDotnetRuntime(0);
    var exports = await getAssemblyExports("MonkeyPaste.Avalonia.Web.dll");
    exports.MonkeyPaste.Avalonia.Web.EmbedInterop.iframeNavigated(hostGuid, url);
}

export async function sendMessageToHost(hostGuid, fn, msg) {
    const { getAssemblyExports } = dotnetRuntime;//await getDotnetRuntime(0);
    var exports = await getAssemblyExports("MonkeyPaste.Avalonia.Web.dll");

    exports.MonkeyPaste.Avalonia.Web.EmbedInterop.receiveMessageFromIframe(hostGuid, fn, msg);
}