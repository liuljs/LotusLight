/*
 * 聯絡資訊管理
 * 友善連結管理
 * 訂閱管理
 * 檔案下載管理
 * 名單管理
 */

$(".btnEdit").click((e) => {
    var tr = e.target.parentElement.parentElement.parentElement;
    $(tr).find('td > input').removeAttr('readonly');
    $(tr).find('td > select').removeAttr('disabled');
    $(tr).find('.btnSave').show();
    $(e.target).hide();
});

function Save(e) {
    var id = e.target.dataset.sn;
    var tr = e.target.parentElement.parentElement.parentElement;

    var requiredCheck = true;
    $(tr).find("input[required], textarea[required]").each((idx, obj) => {
        if (obj.value.length === 0) {
            Swal.fire('請完整填寫所有必填項！');
            requiredCheck = false;
        }
    });
    $(tr).find("input, textarea").each((idx, obj) => {
        if ($(obj).attr("maxlength") !== undefined && obj.value.length >= $(obj).attr("maxlength")) {
            Swal.fire($("label[for=" + obj.id + "]").text() + '已超過字數限制！');
            requiredCheck = false;
        }
    });

    if (!requiredCheck)
        return;
    
    var postData = getPostData(e);

    var xhr = new XMLHttpRequest();
    xhr.open("POST", edtu + "/" + id, true);
    xhr.onload = function () {
        if (this.status === 200 && this.responseText === "OK") {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: '操作成功',
                showConfirmButton: false,
                timer: 1000
            }).then(() => {
                $(tr).find('td > input').attr('readonly', 'readonly');
                $(tr).find('td > select').attr('disabled', 'disabled');
                $(tr).find('.btnEdit').show();
                $(e.target).hide();
            });
        }
        else {
            Swal.fire({
                position: 'top-end',
                icon: 'error',
                title: '操作失敗',
                showConfirmButton: false,
                timer: 1000
            }).then(() => {
                window.location.href = window.location.href;
            });
        }
    };
    xhr.send(postData);
}

// 刪除
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
            xhr.open("POST", delu + "/" + id, true);
            xhr.onload = function () {
                if (this.status === 200 && this.responseText === "OK") {
                    Swal.fire({
                        position: 'top-end',
                        icon: 'success',
                        title: '操作成功',
                        showConfirmButton: false,
                        timer: 1000
                    });

                    e.target.parentElement.parentElement.parentElement.remove(e.target.parentElement.parentElement);
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