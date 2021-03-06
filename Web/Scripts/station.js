﻿$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Collection of results that are connected
        self.results = ko.observableArray();
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#res")[0]);

    // Get a reference to our hub
    var hub = $.connection.resultatServiceHub;

    // Handle updates
    hub.client.newPass = function (res) {
        console.log('newPass');
        vm.results.unshift(res);
    }

    // Start the connection
    $.connection.hub.start().done(function () {
        hub.server.getLatestLocationResult(globalStationId)
            .done(function (res) {
                console.log('getLatest');
                ko.utils.arrayForEach(res, function (item) {
                    vm.results.push(item);
                });
            }).done(function () {
                hub.server.addtoGroup(globalStationId);
            });
    });
});