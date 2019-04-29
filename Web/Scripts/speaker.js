$(function () {

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
    hub.client.addLogMessage = function (etappe, emitId, startNummer, namn, time) {
        if (etappe.startsWith('NewRaceLead')) {
            hub.server.getRaceLeaders()
                .done(function (res) {
                    // Remove items from current list?
                    ko.utils.arrayForEach(vm.results, function (item) {
                        if (!ko.utils.arrayFirst(res, function (lookfor) { return lookfor.Startnumber === item.Startnumber; })) {
                            console.log("removing " + item.Startnumber);
                            ko.utils.arrayRemoveItem(vm.results, lookfor);
                        }
                    });
                    // Add new ones
                    var i = 0;
                    ko.utils.arrayForEach(res, function (item) {
                        if (vm.results()[i].Startnumber !== item.Startnumber)
                            vm.results.splice(i, 0, item);
                        i++;
                    });
                });
        } else if (etappe.startsWith('NewTopResult')) {
            toastr["info"](etappe.substring(12));
        }
    };

    // Start the connection
    $.connection.hub.start().done(function () {
        hub.server.getRaceLeaders()
            .done(function (res) {
                console.log('getLeaders');
                ko.utils.arrayForEach(res, function (item) {
                    vm.results.push(item);
                });
            }).done(function () {
                hub.server.addtoGroup("");
            });
    });
});