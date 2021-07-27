S.article = {
    hub: null,

    init: function () {
        $('.analyze-article .analyze > .button').on('click', S.article.analyze.start);

        //set up signalR hub
        S.article.hub = new signalR.HubConnectionBuilder().withUrl('/articlehub').build();
        S.article.hub.on('update', S.article.analyze.update);
        S.article.hub.on('append', S.article.analyze.append);
        S.article.hub.on('rawhtml', S.article.rawHtml.load);
        S.article.hub.start().catch(S.article.analyze.error);
    },

    analyze: {
        start: function () {
            var url = S.util.location.queryString('url');
            if (url.indexOf('http') != 0 ||
                (url.indexOf('http://') != 0 && url.indexOf('https://') != 0)
            ) {
                S.article.message.error('Invalid URL');
                return;
            }
            $('.analyze-article .analyze').hide();
            $('.analyze-article > .box').css({ height: 400 });

            //send command via SignalR
            S.article.hub.invoke('AnalyzeArticle', url);
        },

        update: function (code, msg) {
            //receive command from SignalR
            var box = $('.analyze-article > .box')[0];
            var div = document.createElement('div');
            div.className = 'cli-line';
            div.innerHTML = '> ' + msg;
            $('.analyze-article .console').append(div);
            box.scrollTop = box.scrollHeight;
        },

        append: function (html) {
            //receive command from SignalR
            $('.article').append(html);
            S.accordion.load();
        },

        error: function (err) {

        }
    },

    rawHtml: {
        lines: [],
        scrollY: 0,
        length:100, 
        empty: {
            top: { hidden: 0, height: 0 },
            bottom: { hidden: 0, height: 0 }
        },
        displayed: [],
        injecting: false,
        mousedown:false,

        load: function (raw) {
            S.article.rawHtml.lines = raw;
            S.article.rawHtml.inject(0, S.article.rawHtml.length);
            $('.accordion.raw-html > .box').on('scroll', S.article.rawHtml.onscroll);
            $('.accordion.raw-html > .box').on('wheel', (e) => { S.article.rawHtml.onscroll(e, 3); });
            $('.accordion.raw-html > .box').on('mousedown', S.article.rawHtml.onmousedown);
            $('.accordion.raw-html > .box').on('mouseup', S.article.rawHtml.onmouseup);
        },

        inject: function (start, length, area) {
            //console.log([start, length, area]);
            var raw = S.article.rawHtml;
            if (raw.injecting || start > raw.lines.length || start < 0) { return; }
            S.article.rawHtml.injecting = true;
            var container = $('.accordion.raw-html .contents');
            var box = $('.accordion.raw-html > .box')[0];
            var emptyTop = container.find('.empty-top');
            var emptyBottom = container.find('.empty-bottom');
            var empty = raw.empty;
            var html = '';
            var h = 0;
            var lines = raw.lines;
            var end = start + length;
            if (end >= lines.length) { end = lines.length; }
            var skipped = [];

            //combine raw html lines
            var added = 0;
            for (var x = start; x < end; x++) {
                if (raw.displayed.indexOf(x) >= 0) {
                    skipped.push(x);
                    added++;
                } else {
                    html += lines[x];
                }
            }
            added = end - start - added; //invert total skipped into total added

            var children;
            if (area == 'top') {
                //prepend
                children = container.children();
                emptyTop.after(html);
            } else {
                //append
                emptyBottom.before(html);
                children = container.children();
            }

            //get height of appended raw HTML content
            for (var x = start; x < end; x++) {
                if (skipped.indexOf(x) >= 0) { continue; }
                var y = x - empty.top.hidden;
                h += $(children[y + 1]).height();
            }

            //get a list of raw HTML lines that are out of range of what's visible
            var deleted = [];
            var deletedElems = [];
            var skip = 1;
            if (area == 'top') {
                skip = added;
                children = container.children();
            }

            for (var x = 0; x < raw.displayed.length; x++) {
                var i = raw.displayed[x];
                if (i < start - raw.length || i >= end + raw.length) {
                    var elem = $(children[x + skip]);
                    if (i < start) {
                        empty.top.height += elem.height();
                        empty.top.hidden++;
                    } else {
                        empty.bottom.height += elem.height();
                        empty.bottom.hidden++;
                    }
                    deleted.push(i);
                    deletedElems.push(elem);
                }
            }

            //add items to displayed list
            var displayed = [];
            var u = 0;
            //add existing items to front of displayed list
            for (var x = 0; x < raw.displayed.length; x++) {
                if (raw.displayed[x] < start) {
                    if (deleted.indexOf(raw.displayed[x]) >= 0) { continue;}
                    displayed.push(raw.displayed[x]);
                } else {
                    break;
                }
            }
            //add new items to displayed list
            for (var x = start; x < end; x++) {
                displayed.push(x);
            }

            //add existing items to end of list
            for (var x = 0; x < raw.displayed.length; x++) {
                if (raw.displayed[x] < end || 
                    deleted.indexOf(raw.displayed[x]) >= 0) { continue; }
                displayed.push(raw.displayed[x]);
            }

            //update empty area height for top & bottom areas
            if (area == 'top') {
                empty.top.height = displayed[0] * 18;
                    empty.top.hidden = displayed[0];
            } else {
                empty.bottom.height = (lines.length - end) * 18; //18px height per line
                if (empty.bottom.height == 0 && empty.bottom.hidden == 0) {
                    empty.bottom.hidden = lines.length - end;
                } else {
                    empty.bottom.hidden = lines.length - displayed[displayed.length - 1] - 1;
                }
            }

            //delete all HTML elements that are out of scope
            for (var x = 0; x < deletedElems.length; x++) {
                var i = raw.displayed.indexOf(deleted[x]);
                deletedElems[x][0].parentNode.removeChild(deletedElems[x][0]);
            }

            //resize top & bottom empty areas
            if (empty.top.height < 0) { empty.top.height = 0; }

            emptyTop.css({ height: empty.top.height });
            emptyBottom.css({ height: empty.bottom.height });
            
            //update global vars
            S.article.rawHtml.displayed = displayed;
            S.article.rawHtml.empty = empty;
            S.article.rawHtml.injecting = false;
            
        },

        onmousedown: function() {
            S.article.rawHtml.mousedown = true;
        },

        onmouseup: function () {
            S.article.rawHtml.mousedown = false;
        },

        onscroll: function (e, c) {
            if (S.article.rawHtml.mousedown == false && c !== 3 && c !== true) { return;}
            var raw = S.article.rawHtml;
            //if (raw.injecting == true) { return; }
            var target = $('.raw-html .box')[0];
            var y = target.scrollTop;
            var y2 = raw.scrollY;
            var bottom = $('.raw-html .empty-bottom');
            var top = $('.raw-html .empty-top');
            if (y > y2) {
                //scrolling down
                if (target.scrollTop + target.clientHeight > bottom.position().top - raw.length) {
                    if (raw.displayed.length > 0 && raw.lines.length > raw.displayed[raw.displayed.length - 1]) {
                        var scrollBottom = $('.raw-html .contents').height() - target.scrollTop - target.clientHeight;
                        var length = raw.lines.length - raw.displayed[raw.displayed.length - 1] - (scrollBottom / 18);
                        if (length < raw.length) { length = raw.length; }
                        if (length > raw.length) { console.log(length); }
                        raw.inject(raw.displayed[raw.displayed.length - 1] + 1, length);
                    } else if (raw.displayed.length == 0) {
                        raw.inject(0, raw.length);
                    }
                }
            } else {
                //scrolling up
                if (target.scrollTop < top.position().top + top.height() + raw.length) {
                    if (raw.displayed[0] - raw.length < 0) {
                        if (raw.displayed[0] - raw.length > -raw.length) {
                            raw.inject(0, raw.length, 'top');
                        }
                    } else {
                        raw.inject(raw.displayed[0] - raw.length, raw.length, 'top');
                    }
                    
                }
            }
            S.article.rawHtml.scrollY = y;
            if (c !== true) {
                //execute a second time to ensure no empty space
                setTimeout(() => { S.article.rawHtml.onscroll(e, true) },100);
            }
        }
    },

    message: {
        error: function (msg) {
            S.article.message.error(msg.toString());
        }
    }
};

S.article.init();