$(document).ready(function () {

    $(`a[href="${window.location.origin + window.location.pathname}"], a[href="${window.location.pathname}"]`).each((idx, obj) => {
        $(obj.parentElement).addClass("active");
    });

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
    var btnCancel = $('.btnCancel');
    var btnChange = $('.btnChange');

    // lightBox 更改密碼
    var changePsw = $('.messageBoxBlock');

    // 打開 lightBox 更改密碼 
    btnChange.on('click', function () {
        changePsw.addClass('active');
    });

    // 要判斷要開啟哪一個 lightBox
    var lightBox = $('.lightBoxBlock');

    // lightBox 關掉
    cancel.on('click', function () {
        lightBox.removeClass('active');
        changePsw.removeClass('active');
    });
    btnCancel.on('click', function () {
        lightBox.removeClass('active');
    });
    // lightBox 打開
    // 要帶入點擊編輯的那一筆資料進入 lightBox 裡
    btnEdit.on('click', function () {
        lightBox.addClass('active');
        // var textArea = lightBox.find('textarea');
        // var lenMax = 20;
        // textArea.attr('maxlength', lenMax);
    });

    btnReset.on('click', function () {
        addForm.find('input').val('');
        addForm.find('textarea').val('');
        addForm.find('.previewImage').attr('src', '')
    });

    var btnShow = $('.addImage');
    var previewBlock = $(".previewImageBlock");
    for (let i = 0; i < btnShow.length; i++) {
        btnShow[i].onchange = function (e) {
            previewBlock.eq(i).html('');
            for (let j = 0; j < btnShow[i].files.length; j++) {
                let readers = new FileReader();
                readers.onload = function (e) {
                    var img = $('<img class="previewImage">').attr('src', e.target.result);
                    previewBlock.eq(i).append(img)
                }
                readers.readAsDataURL(btnShow[i].files[j])
            }
        }
    }

    // Tab 切換標籤
    $('.tabsBlock .tab').on("click", function () {
        // 先移除所有Tab標籤的-on class，在對點擊的Tab標籤加上-on class
        $(this).parent().children().removeClass("active");
        $(this).addClass("active");

        $(".tabsContentsBlock .tabContent").removeClass("active");
        $(".tabsContentsBlock .tabContent").eq($(this).index()).addClass("active");
    });

    // 儲存修改的資料
    bntSave.on('click', function (e) {
        // 找到每一筆點擊儲存的 lightBox 裡的資料，將它們存入資料庫
Save(e);
    });

    // 回復封面圖片為原圖
    btnBack.on('click', function (e) {
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


    // 依捐款方式，判斷捐款金額的上限
    var donateMethods = $('.addDonateMethods');
    var donateAmount = $('.addDonateAmount');
    var stagingBlock = $('.stagingBlock');
    var limitAmount = '';
    donateMethods.on('change', function () {
        // Staging 分期方式，如果付款方式選擇信用卡刷卡
        if ($(this).val() == 01) {
            stagingBlock.html('').append(`
            <label for="stagingMethods">分期方式<span class="requiredBlock">必填</span></label>
            <div class="input-group mb-3">
                <select class="custom-select stagingMethods" id="stagingMethods">
                    <option value="01" selected>一次付清</option>
                    <option value="03">3 期</option>
                    <option value="06">6 期</option>
                    <option value="12">12 期</option>
                </select>
                <div class="input-group-append">
                <label class="input-group-text" for="stagingMethods">選擇方式</label>
                </div>
            </div>
        `)
        } else {
            stagingBlock.html('');
        }
        if ($(this).val() == 4 || $(this).val() == 6 || $(this).val() == 9 || $(this).val() == 10) {
            limitAmount = 20000;
            donateAmount.attr('max', limitAmount).attr('placeholder', '單次金額最低 NT$100；超商繳款上限 2萬元');
        } else {
            limitAmount = '';
            donateAmount.removeAttr('max').attr('placeholder', '單次金額：最低 NT$100');
        }
    })
    // 依捐款用途，選擇其他能填寫寫其他的原因
    var donatePurpose = $('.addDonatePurpose');
    var otherBlock = $('.otherBlock');
    donatePurpose.on('change', function () {
        if ($(this).val() == 15) {
            otherBlock.append(`
        <div class="fieldBlock">
            <input type="text" class="form-control otherPurpose" id="otherPurpose" placeholder="自行填寫捐款用途">
        </div>
        `);
        } else {
            otherBlock.html('');
        }
    });
    // 是否開收據，帶欄位出來
    var addReceipt = $('.addReceipt');
    var receiptBlock1 = $('.receiptBlock1');
    var receiptBlock2 = $('.receiptBlock2');
    addReceipt.on('change', function () {
        if ($(this).val() == 1) {
            receiptBlock1.html('').append(`
        <label for="addReceiptTitle">收據抬頭</label>
        <input type="text" class="form-control addReceiptTitle" aria-label="Sizing example input" aria-describedby="inputGroup-sizing-default" id="addReceiptTitle">
        `);
            receiptBlock2.html('').append(`
        <label for="deliveryMethod">是否寄送<span class="requiredBlock">必填</span></label>
        <div>
            <div class="custom-control custom-radio custom-control-inline">
            <input type="radio" id="deliveryMethod1" name="deliveryMethod" value="1" class="custom-control-input deliveryMethod">
            <label class="custom-control-label" for="deliveryMethod1">每次</label>
            </div>
            <div class="custom-control custom-radio custom-control-inline">
            <input type="radio" id="deliveryMethod2" name="deliveryMethod" value="2" class="custom-control-input deliveryMethod">
            <label class="custom-control-label" for="deliveryMethod2">月寄</label>
            </div>
            <div class="custom-control custom-radio custom-control-inline">
            <input type="radio" id="deliveryMethod3" name="deliveryMethod" value="3" class="custom-control-input deliveryMethod">
            <label class="custom-control-label" for="deliveryMethod3">年寄</label>
            </div>
            <div class="custom-control custom-radio custom-control-inline">
            <input type="radio" id="deliveryMethod4" name="deliveryMethod" value="0" class="custom-control-input deliveryMethod">
            <label class="custom-control-label" for="deliveryMethod4">否</label>
            </div>
        </div>

        `);
        } else {
            receiptBlock1.html('');
            receiptBlock2.html('');
        }

    });

    // 計算捐款總額
    var relatedQuantity = $('.relatedQuantity');
    var relatedTotal;
    var relatedTable = $('.relatedGoodsTable');
    var total = 0;

    relatedQuantity.on('change', function () {
        $(this).parent().parent().parent().parent().find('.relatedGoods').each(function () {
            if ($(this).find('.relatedQuantity').val() == '') {
                $(this).find('.relatedQuantity').val('0');
            }
            var subTotal = parseInt($(this).find('.price').text()) * parseInt($(this).find('.relatedQuantity').val());
            total += subTotal;
        });
        relatedTotal = $(this).parent().parent().parent().parent().find('.relatedTotals');
        relatedTotal.html('');
        relatedTotal.append(total);
        total = 0;
    });

    var shippingMethods = $('.shippingMethods');
    var relatedPicks = $('.addRelatedPicks');

    // shippingMethods 選擇運送方式，帶出填寫的欄位
    relatedPicks.on('change', function () {
        if (relatedPicks.val() == 1) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="shippingAddress">運送地址</label>
                <div class="addressBlock">
                    <div class="twZipCode"></div>
                    <input type="text" class="form-control shippingAddress" placeholder="地址路段" id="shippingAddress">    
                </div>
            </div>
            `);
            $(".twZipCode").twzipcode({
                zipcodeIntoDistrict: true, // 郵遞區號自動顯示在地區
                css: ["addReceiptCity form-control", "addReceiptTown form-control"], // 自訂 "城市"、"地區" class 名稱 
                countyName: "city", // 自訂城市 select 標籤的 name 值
                districtName: "town" // 自訂地區 select 標籤的 name 值
            });
        }
        else if (relatedPicks.val() == 2) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="storeAddress">店到店方式</label>
                <input type="text" class="form-control storeAddress" placeholder="請填寫超商地址或店號" id="storeAddress">    
            </div>
        `);
        }
        else if (relatedPicks.val() == 3) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="shippingNote">備註</label>
                <input type="text" class="form-control shippingNote" placeholder="您有空的時間" id="shippingNote">    
            </div>
        `);
        }
        else if (relatedPicks.val() == 4) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="shippingNote">備註</label>
                <input type="text" class="form-control shippingNote" placeholder="分會地點及您有空的時間" id="shippingNote">    
            </div>
        `);
        }
        else {
            shippingMethods.html('');
        }
    });

    var btnCancelAll = $('.btnCancelAll');
    var checkStatus = $('.checkStatus');
    btnCancelAll.on('click', function () {
        checkStatus.prop('checked', false);
    });

    $('.addDonateMethods').change();
    $('.addReceipt:checked').change();
});

// // table版
function pageTableController(iNow, mainContent, perDis, count) {
    var p_items = mainContent.find('.pageContents tr').not('.tr-hide');

    var pages = Math.ceil(p_items.length / perDis);
    var count = count;
    var nextBlock = mainContent.find('.paginationz .nextBlock');
    var preBlock = mainContent.find('.paginationz .preBlock');
    var pagination = mainContent.find('.paginationz ul');
    function curPage(iNow, pages, count) {
        var pickz = '<li class="active"><a href="">' + iNow + '</a></li>';
        for (let i = 1; i < count; i++) {
            if (iNow - i > 1) {
                pickz = '<li><a href="">' + (iNow - i) + '</a></li>' + pickz;
            }
            if (iNow + i < pages) {
                pickz = pickz + '<li><a href="">' + (iNow + i) + '</a></li>';
            }
        }
        if (iNow == 1) {
            preBlock.html('');
        }
        if (iNow - 3 > 0) {
            pickz = '<li class="unClick"><a href="">...</a></li>' + pickz;
        }
        if (iNow > 1) {
            pickz = '<li><a href="">1</a></li>' + pickz;
            preBlock.html('<a href="" class="btnPre"><p>上一頁</p></a');
        }
        if (iNow + 2 < pages) {
            pickz = pickz + '<li class="unClick"><a href="">...</a></li>';
        }
        if (iNow < pages) {
            pickz = pickz + '<li><a href="">' + pages + '</a></li>';
            nextBlock.html('<a href="" class="btnNext"><p>下一頁</p></a>');
        }
        if (iNow == pages) {
            nextBlock.html('');
        }
        return pickz;
    }
    pagination.html(curPage(iNow, pages, count));
    pagination.on('click', 'li', function (e) {
        e.preventDefault();
        var currentPage = parseInt($(this).find('a').text());
        pagination.html(curPage(currentPage, pages, count));
        // pagination.find(`li:contains('${currentPage}')`).addClass('active').siblings().removeClass('active');
        if (currentPage * perDis > p_items.length) {
            showItems = p_items.slice((currentPage - 1) * perDis);
        }
        else {
            showItems = p_items.slice((currentPage - 1) * perDis, currentPage * perDis);
        }
        p_items.removeClass('active');
        showItems.addClass('active');
        $('.btnNext').on('click', function (e) {
            e.preventDefault();
            currentPage++;
            pagination.find(`li:contains('${currentPage}')`).trigger('click');
        })
        $('.btnPre').on('click', function (e) {
            e.preventDefault();
            currentPage--;
            pagination.find(`li:contains('${currentPage}')`).trigger('click');
        })
    });
    pagination.find('li:first-child a').text(iNow).trigger('click');
};
var pageTable = $('.pageControllers')
pageTableController(1, pageTable, 10, 2);

