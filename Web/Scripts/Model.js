// Namespace
var Kjeringi = {};

// Models

Kjeringi.system = function (sysname, sysId) {
    var self = this;
    self.sysname = sysname;
    self.id = sysId;
}

// ViewModels

Kjeringi.connectedSystemViewModel = function () {
    var self = this;
    self.systems = ko.observableArray();
    self.customRemove = function (sysToRemove) {
        var sysIdToRemove = sysToRemove.id;
        self.systems.remove(function (item) {
            return item.id === sysIdToRemove;
        });
    }
}