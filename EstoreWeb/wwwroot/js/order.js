$(document).ready(function () {
    let status = "";
    const url = window.location.href;
    if (url.includes("?status=")) {
        status = url.split("=")[1];
    }
    loadTableData(status);
});

function loadTableData(status) {
    const url = `/admin/order/getall${status ? '?status=' + status : ''}`.trim();
    console.log(url);
    $('#myTable').DataTable({
        "ajax": { url },
        "columns": [
            { data: 'id' },
            { data: 'name' },
            { data: 'phoneNumber' },
            { data: 'applicationUser.email' },
            { data: 'orderStatus' },
            { data: 'totalOrder' },
            {
                data: "id",
                "render": function (data) {
                    return `
                        <div class="w-auto btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                        </div>
                    `
                }
            }
        ]
    });
}
