// Namespace
var Kjeringi = {};

// Models

Kjeringi.system = function (sysname, sysId, timestamp) {
    var self = this;
    self.sysname = sysname;
    self.id = sysId;
    self.timestamp = timestamp;
}

Kjeringi.logMessage = function (msg) {
    var self = this;
    self.msg = msg;
}

// ViewModels

Kjeringi.connectedSystemViewModel = function () {
    var self = this;
    self.online = ko.observableArray();
    self.offline = ko.observableArray();
    self.log = ko.observableArray();
    self.customRemoveOnline = function (sysToRemove) {
        var sysIdToRemove = sysToRemove.id;
        self.online.remove(function (item) {
            return item.id === sysIdToRemove;
        });
    }
    self.customRemoveOffline = function (sysToRemove) {
        var sysIdToRemove = sysToRemove.id;
        self.offline.remove(function (item) {
            return item.id === sysIdToRemove;
        });
    }
}

Kjeringi.resultatViewModel = function () {
    var self = this;
    self.Namn = ko.observable("Namn");
    self.EmitID = ko.observable("EmitID");
    self.Startnummer = ko.observable("Startnummer");
    self.Medlemmer = ko.observableArray();
    self.Etappetider = ko.observableArray();
    self.TotalTid = ko.observable("Totaltid");
    self.Klasse = ko.observable("Klasse");
    self.PlasseringIKlasse = ko.observable("Plassering");
}

Kjeringi.ticker = function (namn, klasse, emitid, startnummer, medlemmer, etappetider, totaltid, plasseringiklasse) {
    var self = this;
    self.Namn = namn;
    self.EmitID = emitid;
    self.Startnummer = startnummer;
    self.Medlemmer = medlemmer;
    self.Etappetider = etappetider;
    self.TotalTid = totaltid;
    self.Klasse = klasse;
    self.PlasseringIKlasse = plasseringiklasse;
}

Kjeringi.tickerViewModel = function () {
    var self = this;
    self.resultat = ko.observableArray();
}