angular.module("umbraco")
    .controller("b5.LinkedNodesContentAppPackageOptions", function (b5LinkedNodesPackageOptionsResource, localizationService) {
        console.log("config area...");
        var vm = this;

        vm.config = [];
        vm.buttonState = "init";

        localizationService.localize("b5LinkedNodesContentAppInstaller_save").then(function (data) {
            vm.SubmitButtonText = data;
        });

        b5LinkedNodesPackageOptionsResource.getConfig().then(function (response) {
            vm.config = response;
        });

        vm.saveConfig = function () {
            vm.buttonState = "busy";
            b5LinkedNodesPackageOptionsResource.setConfig(vm.config).then(function (response) {
                var status = response;
                if (response === 200) {
                    vm.buttonState = "success";
                    localizationService.localize("b5LinkedNodesContentAppInstaller_saveSuccess").then(function (data) {
                        vm.SubmitButtonText = data;
                    });
                } else {
                    vm.buttonState = "error";
                    localizationService.localize("b5LinkedNodesContentAppInstaller_saveError").then(function (data) {
                        vm.SubmitButtonText = data;
                    });
                }
            });
        };
    });