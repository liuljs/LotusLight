// hamburger control switch
$(".hamburger").on("click", function () {
    $(this).toggleClass("is-active");
});

// hamburger show menu
$(".hamburger").on("click", function () {
    $(".sideNav").toggleClass("showMenu");
});

var addForm = $('.modal-body');
var btnAdd = $('.btnAdd');
var btnReset = addForm.next().find('.btnReset');
var btnEdit = $('.btnEdit');
var btnDel = $('.btnDel');
var btnBack = $('.btnBack');
var btnDelImage = $('.btnDelImage');
var bntSave = $('.btnSave');
var cancel = $('.cancel');
var btncancel = $('.btnCancel');

// 要判斷要開啟哪一個 lightBox
var lightBox = $('.lightBoxBlock');

// lightBox 關掉
cancel.on('click', function () {
    lightBox.removeClass('active');
});
btncancel.on('click', function () {
    lightBox.removeClass('active');
});
// lightBox 打開
// 要帶入點擊編輯的那一筆資料進入 lightBox 裡
btnEdit.on('click', function () {
    lightBox.addClass('active');
});

btnReset.on('click', function () {
    addForm.find('input').val('');
    addForm.find('textarea').val('');
    addForm.find('.previewImage').attr('src', '');
});

// 顯示圖片
var btnShow = $('.addImage');
var previewBlock = $(".previewImageBlock");
for (let i = 0; i < btnShow.length; i++) {

    btnShow[i].onchange = function (e) {
        previewBlock.eq(i).html('');
        for (let j = 0; j < btnShow[i].files.length; j++) {
            let readers = new FileReader();
            readers.onload = function (e) {
                var img = $('<img class="previewImage">').attr('src', e.target.result);
                
                previewBlock.eq(i).append(img);


            };
            readers.readAsDataURL(btnShow[i].files[j]);
        }
    }

    btnShow[i].onclick = function (e) {
        document.body.onfocus = () => {
            //if (!e.value.length) 
            $(btnShow[i]).change();
            document.body.onfocus = null;
        }
    }
}



// 儲存修改的資料
bntSave.on('click', function (e) {
    // 找到每一筆點擊儲存的 lightBox 裡的資料，將它們存入資料庫
    Save(e);
});

// 回復封面圖片為原圖
btnBack.on('click', function () {
    // 要將重新上傳的封面圖片，改回原圖
    $(".titleImage img").attr("src", $(".btnBack").data("lnk"));
    $(".editTitleImage").val('');
});

// 刪除那一則消息資料
btnDel.on('click', function (e) {
    // 找到那一筆要刪除資料的 primary key，將它傳回資料庫
    Delete(e);
});

// 刪除編輯中，已存在資料庫中的那一張圖片
btnDelImage.on('click', function (e) {
    // 指定那張點擊刪除的圖片，將它刪除並回傳資料庫中

});

// 台灣地址 下拉式選單
$(".twZipCode").twzipcode({
    zipcodeIntoDistrict: true, // 郵遞區號自動顯示在地區
    css: ["addReceiptCity form-control", "addReceiptTown form-control"], // 自訂 "城市"、"地區" class 名稱 
    countyName: "city", // 自訂城市 select 標籤的 name 值
    districtName: "town" // 自訂地區 select 標籤的 name 值
});

// 計算捐款總額
var relatedQuantity = $('.relatedQuantity');
var relatedTotal;
var relatedTable = $('.relatedGoodsTable');
var total = 0;

relatedQuantity.on('change', function () {
    $(this).parent().parent().parent().parent().find('.relatedGoods').each(function () {
        if ($(this).find('.relatedQuantity').val() === '') {
            $(this).find('.relatedQuantity').val('0');
        }
        var subTotal = parseInt($(this).find('.price').text()) * parseInt($(this).find('.relatedQuantity').val());
        total += subTotal;
    });
    relatedTotal = $(this).parent().parent().parent().parent().find('.relatedTotal');
    relatedTotal.html('');
    relatedTotal.append(total);
    total = 0;
});

var shippingMethod = $('.shippingMethod');
var shippingMethods = $('.shippingMethods');
var relatedPicks = $('.relatedPicks');

relatedPicks.on('change', function () {
    if ($(this).val() === 0) {
        $(this).parent().parent().parent().find('.shippingMethod').eq($(this).val()).addClass('active').siblings().removeClass('active');
    }
    else if ($(this).val() === 1) {
        $(this).parent().parent().parent().find('.shippingMethod').eq($(this).val()).addClass('active').siblings().removeClass('active');
    }
    else if ($(this).val() === 2) {
        $(this).parent().parent().parent().find('.shippingMethod').eq($(this).val()).addClass('active').siblings().removeClass('active');
    }
    else if ($(this).val() === 3) {
        $(this).parent().parent().parent().find('.shippingMethod').eq($(this).val()).addClass('active').siblings().removeClass('active');
    }
});

