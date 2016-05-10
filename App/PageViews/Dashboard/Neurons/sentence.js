S.neurons = {
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
            for (x = 1; x <= 11; x++) {
                data['txt'+x] = $('.txt-' + x).val();
            }
            console.log(data);
            S.ajax.post('/api/Neurons/AddTrainingData', data, S.ajax.callback.inject);
        },

        runTest: function () {
            S.ajax.post('/api/Neurons/RunTestForSentence', { sentence: $('.txt-test-sentence').val() }, S.ajax.callback.inject);
        }
    }
}

S.neurons.load();