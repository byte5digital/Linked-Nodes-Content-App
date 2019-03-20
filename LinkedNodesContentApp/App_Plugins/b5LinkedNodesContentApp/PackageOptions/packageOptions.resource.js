angular.module('umbraco.resources').factory('b5LinkedNodesPackageOptionsResource',
    function ($q, $http, umbRequestHelper) {
        return {
            getConfig: function () {
                return umbRequestHelper.resourcePromise(
                    $http.get("backoffice/b5LinkedNodes/LinkedNodesContentAppInstallApi/GetConfiguration"),
                    "Failed to retrieve configuration for Linked Nodes Content App");
            },
            setConfig: function (config) {
                return umbRequestHelper.resourcePromise(
                    $http.post("backoffice/b5LinkedNodes/LinkedNodesContentAppInstallApi/SetConfiguration?LinkedNodesConfigModel", config),
                    "Failed to update configuration for Linked Nodes Content App");
            }
        };
    }
);