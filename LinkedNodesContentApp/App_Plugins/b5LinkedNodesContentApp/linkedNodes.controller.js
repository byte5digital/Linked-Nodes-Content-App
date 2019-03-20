angular.module("umbraco")
    .controller("b5.LinkedNodesContentApp", function ($scope, editorState, b5LinkedNodesResource, b5LinkedNodesPackageOptionsResource, localizationService) {
        var vm = this;
        vm.CurrentNodeId = editorState.current.udi;
        vm.IsContent = (editorState.current.mediaLink === undefined);
        vm.Section = vm.IsContent === true ? "Content" : "Media";

        vm.config = [];
        vm.LinkedNodes = [];

        localizationService.localize("b5LinkedNodesContentApp_title" + vm.Section).then(function (data) {
            vm.OverviewTitle = data;
        });

        b5LinkedNodesPackageOptionsResource.getConfig().then(function (response) {
            vm.config = response;
        });

        b5LinkedNodesResource.getLinkedNodes(vm.CurrentNodeId, vm.IsContent).then(function (response) {
            vm.LinkedNodes = response;

            vm.Count = response.length;
            if (vm.Count !== 0) {
                $scope.model.badge = {
                    count: vm.Count,
                    type: "warning"
                };
            }
        });
    });