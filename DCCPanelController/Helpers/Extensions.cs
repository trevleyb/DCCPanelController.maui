namespace DCCPanelController.Helpers;

public static class Extensions {

    public static char GetSortDirection(this bool isAscending) {
        return isAscending ? '▼' : '▲';
    }
}