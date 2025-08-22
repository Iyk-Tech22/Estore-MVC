$(document).ready(function () {
    loadTableData();
});

function loadTableData() {
    $('#myTable').DataTable({
        "ajax": { url: '/admin/product/getall' },
        "columns": [
            { data: 'title' },
            { data: 'isbn' },
            { data: 'author' },
            { data: 'listPrice' },
            { data: 'category.name' },
            {
                data: "id",
                "render": function (data) {
                    return `
                        <div class="w-auto btn-group" role="group">
                            <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2">
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
    const table = $('#myTable').DataTable();
    const product = table.context[0].json.data.find(data => data.id === id);
    const productTitle = product.title || "";

    Swal.fire({
        title: "Are you sure?",
        text: `You want to delete ${productTitle} product`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/admin/product/delete/${id}`,
                type: 'DELETE',
                success: function (response) {
                    if (response.status === "Success") {
                        $('#myTable').DataTable().ajax.reload();
                        toastr.success(response.message);
                    } else {
                        toastr.error(response.message);
                    }
                }
            })
        }
    });
}
