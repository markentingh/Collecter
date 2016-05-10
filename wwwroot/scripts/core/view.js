/// Collector Platform : view.js ///
/// Originally taken from sister project: Websilk (http://www.github.com/websilk/home)
var S = {
    init: function (viewstateid, title) {
        S.ajax.viewstateId = viewstateid;
    },

    window: {
        w: 0, h: 0, scrollx: 0, scrolly: 0, z: 0, absolute: { w: 0, h: 0 }, changed: true,

        pos: function () {
            if (this.changed == false && arguments[0] == null) { return this; } else {
                this.changed = false;
                //cross-browser compatible window dimensions
                this.scrollx = window.scrollX;
                this.scrolly = window.scrollY;
                if (typeof this.scrollx == 'undefined') {
                    this.scrollx = document.body.scrollLeft;
                    this.scrolly = document.body.scrollTop;
                    if (typeof this.scrollx == 'undefined') {
                        this.scrollx = window.pageXOffset;
                        this.scrolly = window.pageYOffset;
                        if (typeof this.scrollx == 'undefined') {
                            this.z = GetZoomFactor();
                            this.scrollx = Math.round(document.documentElement.scrollLeft / this.z);
                            this.scrolly = Math.round(document.documentElement.scrollTop / this.z);
                        }
                    }
                }
                if (arguments[0] == 1) { return this; }

                if (document.documentElement) {
                    this.w = document.documentElement.clientWidth;
                    this.h = document.documentElement.clientHeight;
                }
                if (S.browser.isNS) {
                    this.w = window.innerWidth;
                    this.h = window.innerHeight;
                }
                return this;
            }
        }

    },

    elem: {
        get: function (id) {
            return document.getElementById(id);
        },

        pos: function (elem) {
            var x = 0, y = 0, w = 0, h = 0;
            if (typeof elem != 'undefined' && elem != null) {
                var e = elem;
                while (e.offsetParent) {
                    x += e.offsetLeft + (e.clientLeft || 0);
                    y += e.offsetTop + (e.clientTop || 0);
                    e = e.offsetParent;
                }
                w = elem.offsetWidth ? elem.offsetWidth : elem.clientWidth;
                h = elem.offsetHeight ? elem.offsetHeight : elem.clientHeight;
                if (h == 0) { h = $(elem).height(); }
            }
            return { x: x, y: y, w: w, h: h };
        },

        innerPos: function (elem) {
            var p = this.pos(elem);
            var e = $(elem);
            p.w = e.width();
            p.h = e.height();
            return p;
        },

        offset: function (elem) {
            return {
                y: elem.offsetTop ? elem.offsetTop : elem.clientTop,
                x: elem.offsetLeft ? elem.offsetLeft : elem.clientLeft,
                w: elem.offsetWidth ? elem.offsetWidth : elem.clientWidth,
                h: elem.offsetHeight ? elem.offsetHeight : elem.clientHeight
            }
        },

        top: function (elem) {
            return elem.offsetTop ? elem.offsetTop : elem.clientTop;
        },

        width: function (elem) {
            return elem.offsetWidth ? elem.offsetWidth : elem.clientWidth;
        },

        height: function (elem) {
            return elem.offsetHeight ? elem.offsetHeight : elem.clientHeight;
        },

        fromEvent: function (event) {
            if (S.browser.isIE) {
                return window.event.srcElement;
            } else if (S.browser.isNS) { return event.target; }
            return null;
        }

    },

    css: {
        add: function (id, css) {
            $('#css' + id).remove();
            $('head').append('<style id="css' + id + '" type="text/css">' + css + "</style>");
        },

        remove: function (id) {
            $('head').remove('#css' + id);
        }
    },

    events: {
        doc: {
            load: function () {
                S.browser.get();
            },

            ready: function () {
                S.events.doc.resize.trigger();
                S.accordion.load();
            },

            click: {
                trigger: function (target) {
                    this.callback.execute(target);
                },

                callback: {
                    //register & execute callbacks when the user clicks anywhere on the document
                    items: [],

                    add: function (elem, vars, onClick) {
                        this.items.push({ elem: elem, vars: vars, onClick: onClick });
                    },

                    remove: function (elem) {
                        for (var x = 0; x < this.items.length; x++) {
                            if (this.items[x].elem == elem) { this.items.splice(x, 1); x--; }
                        }
                    },

                    execute: function (target, type) {
                        if (this.items.length > 0) {
                            for (var x = 0; x < this.items.length; x++) {
                                if (typeof this.items[x].onClick == 'function') {
                                    this.items[x].onClick(target, type);
                                }
                            }
                        }
                    }
                }
            },

            scroll: {
                timer: { started: false, fps: 60, timeout: 250, date: new Date(), callback: null },
                last: { scrollx: 0, scrolly: 0 },

                trigger: function () {
                    this.timer.date = new Date();
                    if (this.timer.started == false) { this.start(); }
                },

                start: function () {
                    if (this.timer.started == true) { return; }
                    this.timer.started = true;
                    this.timer.date = new Date();
                    this.callback.execute('onStart');
                    this.go();
                },

                go: function () {
                    if (this.timer.started == false) { return; }
                    this.last.scrollx = window.scrollX;
                    this.last.scrolly = window.scrollY;
                    S.window.scrollx = this.last.scrollx;
                    S.window.scrolly = this.last.scrolly;
                    this.callback.execute('onGo');

                    if (new Date() - this.timer.date > this.timer.timeout) {
                        this.stop();
                    } else {
                        this.timer.callback = setTimeout(function () { S.events.doc.scroll.go(); }, 1000 / this.timer.fps)
                    }
                },

                stop: function () {
                    if (this.timer.started == false) { return; }
                    this.timer.started = false;
                    this.last.scrollx = window.scrollX;
                    this.last.scrolly = window.scrollY;
                    S.window.scrollx = this.last.scrollx;
                    S.window.scrolly = this.last.scrolly;
                    this.callback.execute('onStop');
                },

                callback: {
                    //register & execute callbacks when the window resizes
                    items: [],

                    add: function (elem, vars, onStart, onGo, onStop) {
                        this.items.push({ elem: elem, vars: vars, onStart: onStart, onGo: onGo, onStop: onStop });
                    },

                    remove: function (elem) {
                        for (var x = 0; x < this.items.length; x++) {
                            if (this.items[x].elem == elem) { this.items.splice(x, 1); x--; }
                        }
                    },

                    execute: function (type) {
                        if (this.items.length > 0) {
                            switch (type) {
                                case '': case null: case 'onStart':
                                    for (var x = 0; x < this.items.length; x++) {
                                        if (typeof this.items[x].onStart == 'function') {
                                            this.items[x].onStart();
                                        }
                                    } break;

                                case 'onGo':
                                    for (var x = 0; x < this.items.length; x++) {
                                        if (typeof this.items[x].onGo == 'function') {
                                            this.items[x].onGo();
                                        }
                                    } break;

                                case 'onStop':
                                    for (var x = 0; x < this.items.length; x++) {
                                        if (typeof this.items[x].onStop == 'function') {
                                            this.items[x].onStop();
                                        }
                                    } break;

                            }
                        }
                    }
                }


            },

            resize: {
                timer: { started: false, fps: 60, timeout: 100, date: new Date(), callback: null },

                trigger: function () {
                    this.timer.date = new Date();
                    if (this.timer.started == false) { this.start(); S.window.changed = true; S.window.pos(); }
                },

                start: function () {
                    if (this.timer.started == true) { return; }
                    this.timer.started = true;
                    this.timer.date = new Date();
                    this.callback.execute('onStart');
                    this.go();
                },

                go: function () {
                    S.window.changed = true; S.window.pos();
                    if (this.timer.started == false) { return; }

                    if (new Date() - this.timer.date > this.timer.timeout) {
                        this.stop();
                    } else {
                        this.timer.callback = setTimeout(function () { S.events.doc.resize.go(); }, 1000 / this.timer.fps)
                    }
                },

                stop: function () {
                    if (this.timer.started == false) { return; }
                    this.timer.started = false;
                },

                callback: {
                    //register & execute callbacks when the window resizes
                    items: [],

                    add: function (elem, vars, onStart, onGo, onStop, onLevelChange) {
                        this.items.push({ elem: elem, vars: vars, onStart: onStart, onGo: onGo, onStop: onStop, onLevelChange: onLevelChange });
                    },

                    remove: function (elem) {
                        for (var x = 0; x < this.items.length; x++) {
                            if (this.items[x].elem == elem) { this.items.splice(x, 1); x--; }
                        }
                    },

                    execute: function (type, lvl) {
                        if (this.items.length > 0) {
                            switch (type) {
                                case '': case null: case 'onStart':
                                    for (var x = 0; x < this.items.length; x++) {
                                        if (typeof this.items[x].onStart == 'function') {
                                            this.items[x].onStart();
                                        }
                                    } break;

                                case 'onGo':
                                    for (var x = 0; x < this.items.length; x++) {
                                        if (typeof this.items[x].onGo == 'function') {
                                            this.items[x].onGo();
                                        }
                                    } break;

                                case 'onStop':
                                    for (var x = 0; x < this.items.length; x++) {
                                        if (typeof this.items[x].onStop == 'function') {
                                            this.items[x].onStop();
                                        }
                                    } break;

                                case 'onLevelChange':

                                    for (var x = 0; x < this.items.length; x++) {
                                        if (typeof this.items[x].onLevelChange == 'function') {
                                            this.items[x].onLevelChange(lvl);
                                        }
                                    } break;
                            }
                        }
                    }
                }
            }
        },

        ajax: {
            //register & execute callbacks when ajax makes a post
            loaded: true,

            start: function () {
                this.loaded = false;
                $(document.body).addClass('wait');
            },

            complete: function () {
                S.events.ajax.loaded = true;
                $(document.body).removeClass('wait');
                S.window.changed = true;
                S.events.images.load();
                S.accordion.load();
            },

            error: function (status, err) {
                S.events.ajax.loaded = true;
                $(document.body).removeClass('wait');
            },

            callback: {
                items: [],

                add: function (elem, vars, onStart, onComplete, onError) {
                    this.items.push({ elem: elem, vars: vars, onStart: onStart, onComplete: onComplete, onError: onError });
                },

                remove: function (elem) {
                    for (var x = 0; x < this.items.length; x++) {
                        if (this.items[x].elem == elem) { this.items.splice(x, 1); x--; }
                    }
                },

                execute: function (type) {
                    if (this.items.length > 0) {
                        switch (type) {
                            case '': case null: case 'onStart':
                                for (var x = 0; x < this.items.length; x++) {
                                    if (typeof this.items[x].onStart == 'function') {
                                        this.items[x].onStart();
                                    }
                                } break;

                            case 'onComplete':
                                for (var x = 0; x < this.items.length; x++) {
                                    if (typeof this.items[x].onComplete == 'function') {
                                        this.items[x].onComplete();
                                    }
                                } break;

                            case 'onError':
                                for (var x = 0; x < this.items.length; x++) {
                                    if (typeof this.items[x].onError == 'function') {
                                        this.items[x].onError();
                                    }
                                } break;

                        }
                    }
                }
            }
        },

        images: {
            load: function () {
                imgs = $('img[src!=""]');
                if (!imgs.length) { S.events.images.complete(); return; }
                var df = [];
                imgs.each(function () {
                    var dfnew = $.Deferred();
                    df.push(dfnew);
                    var img = new Image();
                    img.onload = function () { dfnew.resolve(); }
                    img.src = this.src;
                });
                $.when.apply($, df).done(S.events.images.complete);
            },

            complete: function () {
                S.events.doc.resize.trigger();
            }
        }
    },

    ajax: {
        //class used to make simple web service posts to the server
        viewstateId: '', expire: new Date(), queue: [], timerKeep: null, keeping: true,

        post: function (url, data, callback) {
            this.expire = new Date();
            S.events.ajax.start();
            data.viewstateId = S.ajax.viewstateId;
            var options = {
                type: "POST",
                data: JSON.stringify(data),
                dataType: "json",
                url: url,
                contentType: "text/plain; charset=utf-8",
                success: function (d) { callback(d); S.events.ajax.complete(d); S.ajax.runQueue(); },
                error: function (xhr, status, err) { S.events.ajax.error(status, err); S.ajax.runQueue(); }
            }
            S.ajax.queue.push(options);
            if (S.ajax.queue.length == 1) {
                $.ajax(options);
            }
        },

        runQueue: function () {
            S.ajax.queue.shift();
            if (S.ajax.queue.length > 0) {
                $.ajax(S.ajax.queue[0]);
            }
        },

        callback: {
            inject: function (data) {
                if (data.type == 'Collector.Inject') {
                    //load new content from web service
                    var elem = $(data.d.element);
                    if (elem.length > 0 && data.d.html != '') {
                        switch (data.d.inject) {
                            case 0: //replace
                                elem.html(data.d.html);
                                break;
                            case 1: //append
                                elem.append(data.d.html);
                                break;
                            case 2: //before
                                elem.before(data.d.html);
                                break;
                            case 3: //after
                                elem.after(data.d.html);
                                break;
                        }
                    }

                    //add any CSS to the page
                    if (data.d.css != null && data.d.css != '') {
                        S.css.add(data.d.cssid, data.d.css);
                    }

                    //finally, execute callback javascript
                    if (data.d.js != '' && data.d.js != null) {
                        var js = new Function(data.d.js);
                        js();
                    }
                }

                S.events.doc.resize.trigger();
            },
        }
    },

    browser: {
        isIE: false, isNS: false, version: null,

        get: function () {
            var ua, s, i;
            ua = navigator.userAgent;
            s = "MSIE";
            if ((i = ua.indexOf(s)) >= 0) {
                this.isIE = true;
                this.version = parseFloat(ua.substr(i + s.length));
                return;
            }

            s = "Netscape6/";
            if ((i = ua.indexOf(s)) >= 0) {
                this.isNS = true;
                this.version = parseFloat(ua.substr(i + s.length));
                return;
            }

            // Treat any other "Gecko" browser as NS 6.1.

            s = "Gecko";
            if ((i = ua.indexOf(s)) >= 0) {
                this.isNS = true;
                this.version = 6.1;
                return;
            }
        }
    },

    lostSession: function () {
        alert('Your session has been lost. The page will now reload');
        location.reload();
    },

    accordion: {
        load: function () {
            $('.accordion > .title').off('click').on('click', S.accordion.toggle);
        },

        toggle: function () {
            $(this).toggleClass('expanded');
            var box = $(this).parent().find('> .box, > .menu');
            box.toggleClass('expanded');
            if (box.hasClass('expanded')) {
                $('html, body').animate({
                    scrollTop: $(this).offset().top
                }, 700);
            }
        }
    },
    
    popup: {
        element: null,
        elementContainer: null,

        show: function (element) {
            //generate popup interface on-the-fly and move element contents into popup container
            S.popup.hide();
            S.popup.elementContainer = $(element).parent()[0];
            var bg = document.createElement("div");
            var pop = document.createElement("div");
            bg.className = 'popup-bg';
            pop.className = 'popup-container';
            $('body').prepend(bg);
            $('body').prepend(pop);
            $('body > .popup-container').append(element);
            S.popup.element = $('body > .popup-container')[0].children[0];
            S.events.doc.resize.callback.add($('.popup-container')[0], null, S.popup.resize, S.popup.resize, S.popup.resize, null);
            $('.popup-bg').on('click', S.popup.hide);
            S.popup.resize();
        },

        resize: function () {
            //resize popup container to align center with window
            var win = $(window);
            var c = $('.popup-container');
            var x = Math.max(0, (win.width() - c.outerWidth()) / 2);
            var y = Math.max(0, (win.height() - c.outerHeight()) / 2);
            c.css({ top: y, left: x});
        },

        hide: function () {
            //destroy popup interface and move popup contents back to original container
            if ($('body > .popup-container').children.length > 0) {
                //move element back
                $(S.popup.elementContainer).append(S.popup.element);
            }
            $('.popup-bg').off('click');
            S.events.doc.resize.callback.remove($('.popup-container')[0]);
            $('body > .popup-bg, body > .popup-container').remove();
        }
    }
}

S.util = {
    math: {
        numberWithCommas(num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        }
    }

}

//setup jQuery //////////////////////////////////////////////////////////////////////////////////////
$.ajaxSetup({ 'cache': true });

// Window Events ////////////////////////////////////////////////////////////////////////////////////'
$(document).on('ready', function () { S.events.doc.ready(); });
$(document.body).on('click', function (e) { S.events.doc.click.trigger(e.target); });
$(window).on('resize', function () { S.events.doc.resize.trigger(); });
$(window).on('scroll', function () { S.events.doc.scroll.trigger(); });

//raise event after document is loaded
S.events.doc.load();



