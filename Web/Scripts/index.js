$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Whether we're connected or not
        self.connected = ko.observable(false);

        // Collection of machines that are connected
        self.systems = ko.observableArray();
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#systemStatus")[0]);

    // Get a reference to our hub
    var hub = $.connection.resultatServiceHub;

    // Add a handler to receive updates from the server
    hub.client.systemStatusChanged = function (system) {
        console.log(system);
        var systemModel = ko.mapping.fromJS(system);

        // Check if we already have it:
        var match = ko.utils.arrayFirst(vm.systems(), function (item) {
            return item.Id == system.Id;
        });

        if (!match) {
            vm.systems.push(system);
            toastr["info"](system.SystemName, "Added");
        }  else {
            var index = vm.systems.indexOf(match);
            if (system.Connected) {
                toastr["info"](system.SystemName, "Updated");
                vm.systems.replace(vm.systems()[index], system);
            }
            else {
                toastr["error"](system.SystemName, "Offline", { timeOut: 0 });
                vm.systems.remove(
                        function (item) {
                            return item.Id === system.Id;
                        });
            }
        }
    };

    // Start the connection
    $.connection.hub.start().done(function () {
        vm.connected(true);
        hub.server.getConnectedSystems()
            .done(function (connectedsystems) {
                ko.utils.arrayForEach(connectedsystems, function (item) {
                    vm.systems.push(item);
                });
            }).done(function () {
                hub.server.join("Status");
                toastr["success"]("Status", "Connected");
            });
    });
});