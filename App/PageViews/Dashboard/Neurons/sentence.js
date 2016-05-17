S.neurons = {
    timer: null,

    load: function () {
        $('.test-add .btn-add-training').on('click', S.neurons.buttons.addTrainingData);
        $('.test-add .btn-translate-sentence').on('click', S.neurons.buttons.translateSentence);
        $('.btn-run-test').on('click', S.neurons.buttons.runTest);
    },

    buttons: {
        translateSentence: function () {
            var s = $('.txt-sentence').val();
            console.log(s);
            S.ajax.post('/api/Neurons/TranslateSentenceToInputUI', { sentence: s }, S.ajax.callback.inject);
        },

        addTrainingData: function () {
            var data = {testName:'sentences', sentence: $('.txt-sentence').val(), valid: $('.test-type').val() == '1' ? true : false};
            for (x = 1; x <= 13; x++) {
                data['txt'+x] = $('.txt-' + x).val();
            }
            console.log(data);
            S.ajax.post('/api/Neurons/AddTrainingData', data, S.ajax.callback.inject);
        },

        runTest: function () {
            $('.btn-run-test').hide();
            S.neurons.timer = setTimeout(function () {
                $('.test-results').html('<div class="row top"><div class="column title">Please Wait...</div></div>' +
                    '<div class=\"row top\"><div class=\"column\">Neural Network is fine-tuning weights & biases by using training data. This may take a while...</div></div>')
            }, 3000);
            S.ajax.post('/api/Neurons/RunTestForSentence', { sentence: $('.txt-test-sentence').val() },
                function (d) {
                    $('.btn-run-test').show();
                    clearTimeout(S.neurons.timer);
                    S.ajax.callback.inject(d);
                });
        }
    }
}

S.neurons.load();