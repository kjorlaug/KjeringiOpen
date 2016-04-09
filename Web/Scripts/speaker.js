$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Collection of results that are connected
        self.results = ko.observableArray();
        self.incoming = ko.observableArray();

        self.removeIncoming = function (item) {
            var inItems = self.incoming().filter(function(elem){
                return elem.Startnumber() === item.Startnumber(); // find the item with the same id
            })[0];
            self.items.remove(inItems);
        };
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#res")[0]);

    // Get a reference to our hub
    var hub = $.connection.resultatServiceHub;

    // Handle updates
    hub.client.newPass = function (res) {
        console.log(res);
        if (res.Finished) {
            ko.utils.arrayForEach(res.Splits, function (item) {
                if (item.Location == globalStationFinishId)
                    vm.results.unshift(item);
            });
            // Delete from incoming?
            vm.removeIncoming(res);
        } else {
            vm.incoming.push(res);
        }
    }

    // Start the connection
    $.connection.hub.start().done(function () {
        hub.server.getLatestLocationResult(globalStationFinishId)
            .done(function (res) {
                console.log(res);
                ko.utils.arrayForEach(res, function (item) {
                    vm.results.push(item);
                });
            }).done(function () {
                hub.server.getExpected()
                    .done(function(res) {
                        ko.utils.arrayForEach(res, function (item) {
                            vm.incoming.push(item);
                        });
                    })
            }).done(function () {
                hub.server.addtoGroup(globalStationFinishId);
                hub.server.addtoGroup(globalStationIncomingId);
            });
    });
});