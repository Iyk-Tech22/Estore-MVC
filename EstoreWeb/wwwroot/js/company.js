var dataTable;

$(document).ready(function () {
    loadTableData();
});

function loadTableData() {
    dataTable = $('#companyTable').DataTable({
        "ajax": { url: '/admin/company/getall' },
        "columns": [
            { data: 'name' },
            { data: 'streetAddress' },
            { data: 'city' },
            { data: 'state' },
            { data: 'postalCode' },
            { data: 'phoneNumber' },
            {
                data: "id",
                "render": function (data) {
                    return `
                        <div class="w-auto btn-group" role="group">
                            <a href="/admin/company/upsert?companyId=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <button onClick="onDeleteConfirm(${data})" class="btn btn-danger mx-2">
                                <i class="bi bi-trash-fill"></i> Delete
                            </button>
                        </div>
                    `
                }
            }
        ]
    });
}

function onDeleteConfirm(id) {
    const table = $('#companyTable').DataTable();
    const company = table.context[0].json.data.find(data => data.id === id);
    const companyName = company.name || "";

    Swal.fire({
        title: "Are you sure?",
        text: `You want to delete ${companyName}`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/admin/company/delete/${id}`,
                type: 'DELETE',
                success: function (response) {
                    console.log(response)
                    if (response.status === 'Success') {
                        toastr.success(response.message);
                        dataTable.ajax.reload(true);
                    } else {
                        toastr.error(response.message);
                    }
                }
            })
        }
    });
}
