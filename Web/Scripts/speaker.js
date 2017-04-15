$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Collection of results that are connected
        self.results = ko.observableArray();
    };


    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // Get a reference to our hub
    var hub = $.connection.resultatServiceHub;

    // Handle updates
    hub.client.newPass = function (res) {
        console.log(res);
        vm.incoming.push(res);
    }
});