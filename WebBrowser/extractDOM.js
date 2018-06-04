(function () {
    function walk(node) {
        switch (node.nodeType) {
            case 1: //element node
                //get computed style
                var style = window.getComputedStyle(node);

                //convert style.display to int
                var display = 1;
                switch (style.display) {
                    case 'none':
                        display = 0; break;
                    case 'inline':
                        display = 2; break;
                    case 'inline-block':
                        display = 3; break;
                }
                
                //generate parent node object
                var parent = {
                    tag: node.tagName.toLowerCase(),
                    style: {
                        display: display,
                        fontsize: parseInt(style.fontSize.replace('px', '')),
                        fontweight: parseInt(style.fontWeight) >= 700 ? 2 : 1,
                        italic: style.fontStyle == 'italic'
                    },
                    attrs: {},
                    children: []
                };

                //check for invalid tags
                switch (parent.tag) {
                    case "style": case "script": case "svg": case "canvas": case "object":
                    case "embed": case "input": case "select": case "button": case "audio":
                    case "textarea": case "iframe": case "area": case "map": case "noscript":
                        return null;
                }

                //get attributes for node
                var attrs = node.attributes;
                for (var x = 0; x < attrs.length; x++) {
                    switch (attrs[x].name) {
                        case "style": break;
                        default:
                            parent.attrs[attrs[x].name] = attrs[x].value;
                            break;
                    }

                }

                //generate all child nodes
                var children = node.childNodes;
                for (var i = 0; i < children.length; i++) {
                    var child = walk(children[i]);
                    if (child != null) {
                        parent.children.push(child);
                    }
                }
                return parent;

            case 3: //text node
                var val = node.nodeValue.replace('\n', '').replace('\r', '').trim();
                if (val != '') {
                    //replace unknown characters in text
                    val = val.replace(/“/g, '"').replace(/”/g, '"').replace(/[\u{0080}-\u{FFFF}]/gu, '');
                    return { tag: '#text', value: val };
                }
                break;
                
        }
        return null;
    }

    window["__walk"] = walk;
})();

__walk(document.body.parentNode);