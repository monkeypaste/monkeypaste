//import { dotnetRuntime } from './main.js';

export function createIframe(hostGuid, srcUrl) {
    var iframe_elm = globalThis.document.createElement('iframe');

    iframe_elm.addEventListener('load', onIframeLoaded);

    iframe_elm.id = hostGuid;
    iframe_elm.setAttribute('src', srcUrl);
    iframe_elm.setAttribute('crossorigin', true);
    iframe_elm.setAttribute('credentialless', true);

    console.log('created iframe src url "' + srcUrl + '" for host guid "' + hostGuid + '"');

    return iframe_elm;
}
function onIframeLoaded(e) {
    onIframeNavigated(e.srcElement.id, e.srcElement.getAttribute('src'));
}

export function navigateIframe(iframe_elm, url) {
    //let iframe_elm = getEditorIframeElementByHostGuid(hostGuid);
    if (!iframe_elm) {
        console.log('iframe null cannot set src "' + url+'"');
        return;
    }
    iframe_elm.setAttribute('src', url);
}


function getEditorIframeElementByHostGuid(hostGuid) {
    return globalThis.document.getElementById(hostGuid);
}

export function sendMessageToIframe(iframe_elm, msg) {
    if (!msg) {
        return;
    }
    //let editor_elm = getEditorIframeElementByHostGuid(hostGuid);
    if (!iframe_elm) {
        console.log('iframe null cannot send msg "' + msg+'"');
        return;
    }
    iframe_elm.contentWindow.postMessage(msg);
    console.log('msg to editor: ' + msg)
}

window.addEventListener('message', onReceivedIframeMessage, false);

function onReceivedIframeMessage(e) {
    console.log('recevd iframe message: ' + e);
    sendMessageToHost(e.guid, e.type, e.data);
}

export async function onIframeNavigated(hostGuid, url) {
    const { getAssemblyExports } = await globalThis.getDotnetRuntime(0);
    var exports = await getAssemblyExports("MonkeyPaste.Avalonia.Web.dll");
    exports.MonkeyPaste.Avalonia.Web.EmbedInterop.iframeNavigated(hostGuid, url);
}

export async function sendMessageToHost(hostGuid, fn, msg) {
    const { getAssemblyExports } = await globalThis.getDotnetRuntime(0);
    var exports = await getAssemblyExports("MonkeyPaste.Avalonia.Web.dll");

    exports.MonkeyPaste.Avalonia.Web.EmbedInterop.receiveMessageFromIframe(hostGuid, fn, msg);
}

export function getWindow() {
    //let result = `${globalThis.window.innerWidth},${globalThis.window.innerHeight}`;
    return globalThis.window;
}