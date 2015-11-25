S.editor = {
    load:function(){
        S.blocks.fields.init();
        $('.mailbody').delegate('.block', 'mouseenter', S.blocks.mouseEnter);
        $('.select-box').on('mouseleave', S.blocks.mouseLeave);
    },

    hideModals: function () {
        $('.tools .modal').hide();
        S.events.doc.click.callback.remove('modal');
    },

    showModal(name) {
        S.editor.hideModals();
        $('.tools .' + name).show();
        setTimeout(function () {
            S.events.doc.click.callback.add('modal', null,
            function (target) {
                if ($(target).parents('.tools').length == 0) {
                    S.events.doc.click.callback.remove('modal');
                    S.editor.hideModals();
                }
            });
        }, 100);
    }
}

S.blocks = {
    current:null,
    mouseEnter: function (e) {
        var boxPos = S.elem.pos(e.currentTarget);
        $('.select-box').css({ width: boxPos.w, height: boxPos.h, left: boxPos.x, top: boxPos.y }).show();
        $('.select-box .closeblock').css({ left: boxPos.w - 47 });
        $('.select-box .editblock').attr('onclick', 'S.blocks.fields.load(\'' + e.currentTarget.id.substr(5) + '\')');
        $('.select-box .addblock').attr('onclick', 'S.blocks.list(\'' + e.currentTarget.id + '\')');
        $('.select-box .closeblock').attr('onclick', 'S.blocks.remove(\'' + e.currentTarget.id.substr(5) + '\')');
        if (e.currentTarget.getAttribute("nofields") == "true") {
            $('.select-box .editblock').hide();
        } else {
            $('.select-box .editblock').show();
        }
        
    },

    mouseLeave: function (e) {
        $('.select-box').hide();
    },

    list: function (elem) {
        S.blocks.current = elem;
        S.editor.showModal('block-list');
    },

    add: function (id) {
        var blocks = $('.mailbody > .block');
        var index = blocks.length;
        var elem = S.blocks.current;
        if (elem != null) {
            elem = '#' + elem;
            var c = $(elem)[0];
            for (x = 0; x < blocks.length; x++) {
                if (blocks[x] == c) {
                    index = x;
                }
            }
        }

        S.ajax.post('/api/Editor/AddBlock', { id: id, element:elem || 'last', index:index },
            function (data) {
                S.editor.hideModals();
                S.ajax.callback.inject(data);
            });
    },

    remove: function (id) {
        $('#block' + id).remove();
        S.ajax.post('/api/Editor/RemoveBlock', { id: id },function (data) {});
    },

    fields: {
        current: '',

        init: function () {
            //setup form to post via AJAX
            $(".block-fields > form").submit(function () {
                S.ajax.post('/api/Editor/SaveBlockFields', { id: S.blocks.fields.current, fields: $(".block-fields > form").serialize() }, S.ajax.callback.inject);
                return false;
            });
        },

        load: function (id) {
            S.blocks.fields.current = id;
            S.ajax.post('/api/Editor/LoadBlockFields', { id: id },
            function (data) {
                S.ajax.callback.inject(data);
                S.editor.showModal('block-fields');
                S.blocks.fields.init();
            });
        }
    }
}

S.template = {
    save: function (id) {
        S.ajax.post('/api/Editor/SaveTemplate', { id: id },
            function (data) {
                $('.savemsg').css({ opacity: 1 }).show().delay(5000).animate({ opacity: 0 }, 1000, function () { $(this).hide(); });
            });
    }
}

S.editor.load();