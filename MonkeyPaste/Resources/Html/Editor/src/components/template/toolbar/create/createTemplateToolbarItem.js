var CanUserAddTemplate = true;

function showTemplateToolbarContextMenu(tb) {
    if (!CanUserAddTemplate) {
        log('user add template currently disabled (selection must be on/within template)');
        return;
	}
    let tb_rect = tb.getBoundingClientRect();
    let x = tb_rect.left;
    let y = tb_rect.bottom;

    let allTemplateDefs = getAvailableTemplateDefinitions()
    var cm = [];

    for (var i = 0; i < templateTypesMenuOptions.length; i++) {
        let tmi = templateTypesMenuOptions[i];

        let allTemplateDefsForType = allTemplateDefs.filter(x => x.templateType.toLowerCase() == tmi.label.toLowerCase());

        tmi.submenu = allTemplateDefsForType.map(function (ttd) {
            return {
                icon: ' ',
                iconBgColor: ttd.templateColor,
                label: ttd.templateName,
                action: function (option, contextMenuIndex, optionIndex) {
                    createTemplate(ttd);
                },
            }
        });

        if (allTemplateDefsForType.length > 0) {
            tmi.submenu.push({ separator: true });            
        }
        tmi.submenu.push(
            {
                icon: 'fa-solid fa-plus',
                iconFgColor: 'lime',
                label: 'New...',
                action: function (option, contextMenuIndex, optionIndex) {
                    createTemplate(null, tmi.label.toLowerCase());
                },
            }
        )
        cm.push(tmi);
    }

    superCm.createMenu(cm, { pageX: x, pageY: y });
}

function hideTemplateToolbarContextMenu() {
    //var rgtClickContextMenu = document.getElementById('templateToolbarMenu');
    //rgtClickContextMenu.style.display = 'none';
    superCm.destroyMenu();
}

function isShowingTemplateToolbarMenu() {
    return superCm.isOpen();
}


function getTemplateIconStr(isEnabled) {
    if (isEnabled) {
        //black
        return 'data:image/png;base64,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';
        //yellow
        //return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAaVBMVEX/////wQf/vwD/24f/3Iz///3/vQD/ykL/yTj/yT3/0WH/0F7/0Fv/y0T/y0f/yTr/35T/wxT/8tP/6Lj/5az/7sj/9uX/46P/1G7/677/57P/2oL/xSj/8M//1nj//fb//O//4Zz/zVBsBjAmAAAEzklEQVR4nO2di3qiMBBGIUpa1168tFZ70933f8hl0KqVWCuZyfzhm/MAfJ7vhAmwCy0KwzAMwzAMwzAMwzAMwzAMWKq3QSzDyXymrXGe6ta7eLwvl8/aKmGqW1fy4Pz0SdsmAJ9g4zhaaAudwirYOIJl5Bas8QNtqWMEBLEURQRrRZiFKiRYK4KMGzHB0j1ouzXICdYR59p2haxg6d619YQF64gfPRcs3ae24KOsYOmmyoLCBUvtDSOBYOk176TEl2hjqHhdw1WwvuetOXcsN9ET5Cno/PJ1tvh4Gp9xdEM1QZ6C/m29O+Dmj4cyZCroV0fHfAkpahlyFfw+KEOKSoZcBU9//bh9WB1Dtn3w9MCbdkQVQ7Ztov3j71pH1jBk2+h9++n2UyuigiHfpZrftA7+AWDIeKnm162jL/QNOS+2Aw1n6oasF9uB83Cubch7uxS4qF4qz9JqxHq71L5/Xyvvh+z3g/40YjthUkOBG96TM7F9FiY1ZF6i25/vjhVXuvcWUv/4sl+o60/d+0OJglvF6XC2WS+eP53uPb7gUzV34TlNGkOxgpdJY5jiuaiqoWLBNIZJHvxqGuoKJjBUXaIpDDWHTBJD7YLihuoFxQ3VCwob6i9RacMbAEFJwwpCUNIQQ1DOEKSgoCHCkGkQMoQpKGUIsU3skDHEKShjiFRQxhCpoIQh0JBp4DcEE2Q3RCvIbwg1ZBp4DfEKMhtibRM7WA3vAQU5DRGXaMlqCFmQ0RC0IKMhqiCXYQW6REs2Q9iCXIa4BZkMgQvyGCIX5DCE3SZ2xBtiF4w3RC8YbwgvGGkIvNHviTPELxhpmEHBOMMcCkYZPmQh2H77q2cFS/dQ9bugu+laMIshE/Ntk1wEOxccB//POBzdC75lInjfVfC574LFNIuTMOIDSn+zSNh9yBRV3wsWrzkkjCgYeh8cj6iPmOWwSKMKBl4lhsPdd73YbsA/DeMKFsUEfZW6m6iCRTEAN4y4ksnDMLog+iqNLxh+XRqG2CHTgLxbcBSE3vFZChbBTzFhELnRH0A9EbkK1mirhGHYJvYEv/umDc+Q+eId70zkLFggbhi8BWsmYIqMQ+aLJZQie0EC6aG3GwkIFkX4W6EaMA8ZPEWBc/ALjIUqVpBAqChYkLhTV3QjwYKEdkXhgoRuRfGChGZF0SFzQG+iCm30bbQqJiqop5hgyBzQWKgJCxLpKyYtSKTeNJJsE99JWzF5QSJlRYWCRLqKiYfMgVQT1d0qCaaqqLRE0ym6R0XBFAtVWVC+ouoS3SK7aSgOmQOSFQEKEnIVIQoSUhVBChIyExWmICFREaggwa+ovg+ewr1Q4QS5K4It0S2cmwbUkDnAVxGyIMFVEbQgwVMRcMgc4JiowAWJ+IrQBYlYRfCCRNxCzUAwriL8Et3SfdPIoiDRtWImBYluFbMpSHSpmFFB4vqJmlVB4tqKmRUkrlPMriBxzULNUvCaihku0S2/3TQyLUj8rmK2BYnfVMy4IHG5YuaClydq9oKXKvZA8GfFXgj+tFB7Inj+hSLf+btqcEyCf7nXL7V/FyOz91ZGX660fxUvL+VxR+fdpDcrdM9q7Lx3jv7gtPv32j8/oprNJ8PB5HXWTz3DMAzDMAzDMAzDMAzDMHrEf9VjaGNa1FYRAAAAAElFTkSuQmCC";
    } else {
        //gray
        return 'data:image/png;base64,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';
    }
}

function enableCanCreateTemplateToolbarItem() {
    CanUserAddTemplate = true;
    document.getElementById('templateToolbarButton').style.color = 'black';
}

function disableCanCreateTemplateToolbarItem() {
    CanUserAddTemplate = false;
    document.getElementById('templateToolbarButton').style.color = 'lightgray';

}