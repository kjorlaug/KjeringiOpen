$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Collection of results that are connected
        self.incoming = ko.observableArray();
        self.results = ko.observableArray();

        self.removeIncoming = function (item) {
            self.incoming.remove(function (i) {
                return i.Startnumber == item.Startnumber;
            });
        };

        self.sortFunction = function (a, b) {
            return a.EsitmatedTicks > b.EsitmatedTicks ? 1 : -1;
        };

        self.sortedIncoming = ko.dependentObservable(function () {
            return this.incoming.slice().sort(this.sortFunction);
        }, self);

    };


    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#incoming")[0]);

    // Get a reference to our hub
    var hub = $.connection.resultatServiceHub;

    // Handle updates
    hub.client.newPass = function (res) {
        if (res.Finished) {
            // Delete from incoming?
            vm.removeIncoming(res);
            ko.utils.arrayForEach(res._splits, function (item) {
                if (item.Location == globalStationFinishId)
                    vm.results.unshift(item);
            });

        } else {
            vm.incoming.push(res);
        }
    }

    // Start the connection
    $.connection.hub.start().done(function () {
        hub.server.getLatestLocationResult(globalStationFinishId)
            .done(function (res) {
                ko.utils.arrayForEach(res, function (item) {
                    vm.results.push(item);
                });
            }).done(function () {
                hub.server.getExpected()
                    .done(function(res) {
                    ko.utils.arrayForEach(res, function (item) {
                        vm.incoming.push(item);
                    });
            }).done(function () {
                hub.server.addtoGroup(globalStationFinishId);
                hub.server.addtoGroup(globalStationIncomingId);
            });
            });
    });
});