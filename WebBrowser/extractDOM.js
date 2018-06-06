(function () {
    var knownAttrs = [];

    function getDOM(node) {
        var dom = walk(node);
        return { a: knownAttrs, dom: dom };
    }

    function walk(node) {
        switch (node.nodeType) {
            case 1: case 3: //element node or text node
                //get computed style
                var style;
                var name = null;

                if (node.nodeType == 1) {
                    style = window.getComputedStyle(node);
                    name = node.tagName.toLowerCase();
                } else {
                    style = window.getComputedStyle(node.parentNode);
                    name = "#text";
                }
                
                //convert style.display to int
                var display = 1;
                switch (style.display) {
                    case 'none':
                        switch (name) {
                            //hidden elements that are required as part of the DOM
                            case "head": case "title": case "meta": case "#text":
                                display = 0;
                                break;
                            default: return null;
                        }
                    case 'inline':
                        display = 2; break;
                    case 'inline-block':
                        display = 3; break;
                }

                //generate parent node object
                var parent = {
                    t: name,
                    s: [display,                                    //[0] display
                        parseInt(style.fontSize.replace('px', '')), //[1] font-size
                        parseInt(style.fontWeight) >= 700 ? 2 : 1,  //[2] font-weight
                        style.fontStyle == 'italic' ? 1 : 0         //[3] italic
                    ]
                };

                if (node.nodeType == 1) {
                    parent.a = {};
                    parent.c = [];

                    //check for invalid tags
                    switch (parent.t) {
                        case "style": case "script": case "svg": case "canvas": case "object":
                        case "embed": case "input": case "select": case "button": case "audio":
                        case "textarea": case "iframe": case "area": case "map": case "noscript":
                            return null;
                    }

                    //get attributes for node
                    var attrs = node.attributes;
                    for (var x = 0; x < attrs.length; x++) {
                        switch (attrs[x].name) {
                            case "style": case "id": case "tabindex": case "index": break;
                            default:
                                if (attrs[x].name.indexOf('data-') == 0) { break; }
                                if (attrs[x].name.indexOf('aria-') == 0) { break; }
                                if (knownAttrs.indexOf(attrs[x].name) < 0) {
                                    //add name to known attributes list
                                    knownAttrs.push(attrs[x].name);
                                }
                                parent.a[knownAttrs.indexOf(attrs[x].name)] = attrs[x].value;
                                break;
                        }

                    }

                    //generate all child nodes
                    var children = node.childNodes;
                    for (var i = 0; i < children.length; i++) {
                        var child = walk(children[i]);
                        if (child != null) {
                            parent.c.push(child);
                        }
                    }

                    return parent;

                } else if (node.nodeType == 3) {
                    //#text node
                    var val = node.nodeValue.replace('\n', '').replace('\r', '').trim();
                    if (val != '') {
                        //replace unknown characters in text
                        parent.v = val.replace(/“/g, '"').replace(/”/g, '"').replace(/[\u{0080}-\u{FFFF}]/gu, '');
                        return parent;
                    }
                }
                break;
        }
        return null;
    }

    window["__getDOM"] = getDOM;
})();

__getDOM(document.body.parentNode);