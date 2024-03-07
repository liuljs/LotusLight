function Save(e) {
    var link = $(e.target).parents('.card').find(".editLinkURL").val();
    var sort = $(e.target).parents('.card').find('.editSort').val();
    var id = e.target.dataset.sn;

    var postData = new FormData();
    postData.append("lnk", link);
    postData.append("sort", sort);

    var xhr = new XMLHttpRequest();
    xhr.open("POST", window.location.origin + "/LotusLightAdmin/Act/EditCarousel/" + id, true);
    xhr.onload = function () {
        if (this.status === 200 && this.responseText === "OK") {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: '操作成功',
                showConfirmButton: false,
                timer: 1000
            });
        }
        else {
            Swal.fire({
                position: 'top-end',
                icon: 'error',
                title: '操作失敗',
                showConfirmButton: false,
                timer: 1000
            });
        }
    };
    xhr.send(postData);
}

function Delete(e) {
    Swal.fire({
        icon: 'warning',
        title: '確定要刪除嗎?',
        text: '刪除後不可複原，請再次確認是否要刪除？',
        showConfirmButton: true,
        showCancelButton: true,
        confirmButtonText: '確定',
        cancelButtonText: '取消'
    }).then((result) => {
        if (result.isConfirmed) {
            var id = e.target.dataset.sn;

            var xhr = new XMLHttpRequest();
            xhr.open("POST", window.location.origin + "/LotusLightAdmin/Act/DeleteCarousel/" + id, true);
            xhr.onload = function () {
                if (this.status === 200 && this.responseText === "OK") {
                    Swal.fire({
                        position: 'top-end',
                        icon: 'success',
                        title: '操作成功',
                        showConfirmButton: false,
                        timer: 1000
                    });

                    e.target.parentElement.parentElement.parentElement.parentElement.remove(e.target.parentElement.parentElement.parentElement);
                }
                else {
                    Swal.fire({
                        position: 'top-end',
                        icon: 'error',
                        title: '操作失敗',
                        showConfirmButton: false,
                        timer: 1000
                    });
                }
            };
            xhr.send();
        } 
    }); 
}