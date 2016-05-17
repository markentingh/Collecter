S.topics.edit = {
    edited: false,
    dropzone: null,

    load: function () {
        $('#btnaddsection').on('click', function () { S.topics.edit.buttons.addSection('section', 0); });
        $('.btn-select-all-images').on('click', S.topics.edit.media.toggleSelect);
        $('.btn-gallery-toggle').on('click', S.topics.edit.media.toggleGallery);
        S.topics.edit.media.refresh();

        //init dropzone library
        Dropzone.autoDiscover = false;

        S.topics.edit.dropzone = new Dropzone(document.body, {
            url: '/api/Topics/Upload?v=' + S.ajax.viewstateId,
            previewsContainer: ".topic-media .upload-list",
            clickable: ".topic-media .btn-upload",
            paramName: 'file',
            maxFilesize: 4,
            uploadMultiple: true,
            thumbnailWidth:100,
            thumbnailHeight: 80,
            parallelUploads: 1
                    , init: function () {
                        this.on('sending', function () {
                            $('.topic-media .dropzone').addClass('uploading');
                        });

                        this.on('complete', function (file) {
                            this.removeFile(file);
                        });

                        this.on('queuecomplete', function () {
                            S.ajax.post('/api/Topics/SaveUpload', {}, S.ajax.callback.inject);
                            $('.topic-media .dropzone').addClass('uploaded').removeClass('uploading');
                        });
                    }
        });
    },

    buttons: {
        editTopic: function (className) {
            console.log(className);
            $('.accordion.' + className + ' .preview, .' + className + ' .btn-cancel').hide();
            $('.accordion.' + className + ' .edit').show();
            autosize.update($('#' + className + '-content'));
        },

        previewTopic: function (className) {
            var title = $('.accordion.' + className + ' .txt-title').val();
            var content = $('.accordion.' + className + ' .txt-content').val();
            var htm = marked(content
                    .replace(/\n{3,}/g, '\n\n!b!\n\n')
                    .replace(/\n{3,}/g, '\n\n!b!\n\n')
                ).replace(/\<p\>\!b\!\<\/p\>/g, '<br/>');
            $('.accordion.' + className + ' .edit, .preview .nopreview').hide();
            $('.accordion.' + className + ' .preview, .preview .ispreview').show();
            $('.accordion.' + className + ' .section-contents')[0].innerHTML = htm;
            $('.accordion.' + className + ' > .title').html(title);
        },

        addSection: function (group, index) {
            var sections = $('.topic-section');
            var section = $('.' + group + index + ' .topic-section');
            var count = sections.length;
            var element = '.' + group + (index + 1);
            var after = false;
            if (section[0] == sections[sections.length - 1]) { after = true; element = '.' + group + index; }
            var options = { element: element, after: after, title: "New Section", content: "", index: index, count: count };
            S.ajax.post('/api/Topics/NewTopicSection', options, function () {
                S.ajax.callback.inject(arguments[0]);
                $('html, body').animate({
                    scrollTop: $('.' + group + (count + 1)).offset().top
                }, 700);
            });
        },

        removeSection: function (group, index) {
            if (confirm('Do you really want to delete this section?')) {
                $('.section' + index).remove();
                S.ajax.post('/api/Topics/RemoveSection', {index: index}, S.ajax.callback.inject);
            }
            
        },

        saveChanges: function (groupName) {
            if (S.topics.edit.edited == false) { return; }
            var i = 1;
            var e = 0;
            var data = new Array();
            var obj;
            var sections = $('.topic-section');
            var section;
            var id = '';
            var title = '';
            $('.btn-savechanges').hide();
            S.topics.edit.edited = false;
            console.log(sections);
            for (var s = 0; s < sections.length; s++) {
                console.log(s);
                section = sections[s];
                console.log(section);
                id = section.className.replace('topic-section id-' + groupName, '');
                title = $(section).find('.txt-title').val();
                console.log('id = ' + id);
                data.push({
                    title: title,
                    content: $(section).find('.txt-content').val(),
                    id: id
                });
                //update title
                $('.' + groupName + id + ' > .title').html(title);
            }

            console.log(data);
            S.ajax.post('/api/Topics/SaveTopic', { sections: JSON.stringify(data) }, function () { });
        }
    },

    texteditor: {
        keyDown: function (e) {
            if (S.topics.edit.edited == false) {
                $('.li-savechanges, .btn-savechanges').show();
                S.topics.edit.edited = true;
                autosize.update($(e));
            }
        },

        autoSize: function () {
            var ta = document.querySelector('textarea');
            
            //remove any existing autosizes
            var evt = document.createEvent('Event');
            evt.initEvent('autosize:destroy', true, false);
            ta.dispatchEvent(evt);

            //add new autosizes to all textareas on the page
            autosize($('textarea'));
        }
    },

    media: {
        refresh: function () {
            //updates media items
            $('.media-list .chk').off().on('click', S.topics.edit.media.check);
        },

        check: function () {
            //select a media item
            var chk = $(this).find('input[type=checkbox]');
            if (chk.is(":checked") == true) {
                $(this).addClass('checked');
            } else {
                $(this).removeClass('checked');
            }
            S.topics.edit.media.selected();
        },

        selected: function () {
            //check if any other images are selected
            if ($('.media-list .chk input[type=checkbox]:checked').length > 0) {
                $('.btn-delete-selected-images').show();
            } else {
                $('.btn-delete-selected-images').hide();
            }
        },

        toggleSelect: function(){
            if ($('.media-list .chk input[type=checkbox]:checked').length > 0) {
                //deselect
                $('.media-list .chk input[type=checkbox]:checked').prop("checked", false);
                $('.media-list .chk').removeClass('checked');
            } else {
                //select all
                $('.media-list .chk input[type=checkbox]').prop("checked", true);
                $('.media-list .chk').removeClass('checked').addClass('checked');
            }
            S.topics.edit.media.selected();
        },

        toggleGallery: function () {
            var list = $('.media-list');
            if(list.hasClass('gallery')){
                list.removeClass('gallery');
            } else {
                list.addClass('gallery');
            }
            
        }
    }
};

S.topics.edit.load();