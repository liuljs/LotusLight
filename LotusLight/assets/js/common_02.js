function googleTranslateElementInit() {
    var areas = 'google_translate_element';
    new google.translate.TranslateElement({ pageLanguage: 'zh-TW', includedLanguages: 'zh-TW,zh-CN,en' }, areas);
}
$(document).ready(function () {

    $(`a[href="${window.location.origin + window.location.pathname}"], a[href="${window.location.pathname}"]`).each((idx, obj) => {
        $(obj.parentElement).addClass("active");
    });
    // 判斷是否為 LINE 內建瀏覽器
    if (navigator.userAgent.indexOf("Line") > -1) {
        location.href = window.location.href + "?" + "openExternalBrowser=1";
    };

    // hamburger control switch
    $(".hamburger").on("click", function () {
        $(this).toggleClass("is-active");
    });
    // hamburger show menu
    $(".hamburger").on("click", function () {
        $(".phoneHeader").toggleClass("active");
        if ($(".phoneHeader").hasClass("active")) {
            $(document.body).css('overflow', 'hidden');
        } else {
            $(document.body).css('overflow', 'auto');
        }
    });

    var windowsWidth = $(window).width() + 17;
    var rwdWidth = 1023;

    if (windowsWidth > rwdWidth) {
        $('.header').find('.bottomBlock .transformBlock').append(`
            <div id="google_translate_element"></div>
        `);
    } else {
        $('.phoneHeader').find('.bottomBlock .transformBlock').append(`
            <div id="google_translate_element"></div>
        `);

    }
    $(window).resize(function () {
        var currentWidth = $(window).width() + 17;
        if (windowsWidth < rwdWidth && currentWidth >= rwdWidth) {
            location.reload();
        }
        if (windowsWidth >= rwdWidth && currentWidth < rwdWidth) {
            location.reload();
        }
    })
    //Top 顯示與功能
    var btnGoTop = $('.btnGoTop'); // btnGoTop 回到頂端
    btnGoTop.click(function () {
        $('html,body').animate({
            scrollTop: 0
        }, 1000);
    })
    $(window).scroll(function () {
        let s = $(document).scrollTop();
        if (s > 1000) {
            btnGoTop.css({
                'opacity': '1',
                'visibility': 'visible'
            });
        } else {
            btnGoTop.css({
                'opacity': '0',
                'visibility': 'hidden'
            })
        }
    });

    // Carousel for Index 首頁自動輪播
    var indexPage = $('.indexPage');
    var carousels = indexPage.find('.carouselBlock .carouselWholeBlock');
    var carouselItem = carousels.find('.carouselPart');
    var timer = null;
    var iNow = 0;
    carouselItem.eq(0).addClass('active');
    function AutoCarousels() {
        iNow++;
        if (iNow > carouselItem.length - 1) {
            iNow = 0;
        }
        carouselItem.eq(iNow).addClass('active').siblings().removeClass('active');
        timer = setTimeout(AutoCarousels, 4000);
    }
    timer = setTimeout(AutoCarousels, 4000);
    carousels.hover(
        function () {
            clearTimeout(timer);
        },
        function () {
            clearTimeout(timer);
            timer = setTimeout(AutoCarousels, 4000);

        }
    );

    // lightBox for Donate 
    var lightBox = $('.lightBoxBlock.donateNormal');
    // cancel
    var cancel;
    // 打開 lightBox
    var btnDonate = $('.btnDonate');
    btnDonate.on('click', function (e) {
        lightBox.addClass('active');
        e.preventDefault();
        if (lightBox.hasClass("active")) {
            $(document.body).css('overflow', 'hidden');
        } else {
            $(document.body).css('overflow', 'auto');
        }
        // lightBox 關掉
        cancel = lightBox.find('.cancel');
        cancel.on('click', function () {
            lightBox.removeClass('active');
            $(document.body).css('overflow', 'auto');
        });
    });

    // 重置
    var btnReset = lightBox.find('.btnReset');
    btnReset.on('click', function () {
        lightBox.find('input').val('');
    });

    // 台灣地址 下拉式選單
    $(".twZipCode").twzipcode({
        zipcodeIntoDistrict: true, // 郵遞區號自動顯示在地區
        css: ["addReceiptCity form-control", "addReceiptTown form-control"], // 自訂 "城市"、"地區" class 名稱 
        countyName: "city", // 自訂城市 select 標籤的 name 值
        districtName: "town" // 自訂地區 select 標籤的 name 值
    });
    // 依捐款方式，判斷捐款金額的上限
    var donateMethods = lightBox.find('.donateMethods');
    var donateAmount = lightBox.find('.donateAmount');
    var stagingBlock = lightBox.find('.stagingBlock');
    var limitAmount = '';
    donateMethods.on('change', function () {
        // Staging 分期方式，如果付款方式選擇信用卡刷卡
        if ($(this).val() == 01) {
            stagingBlock.html('').append(`
            <label for="stagingMethods">分期方式</label>
            <div>
                <select class="stagingMethods" id="stagingMethods">
                    <option value="01" selected>一次付清</option>
                    <option value="03">3 期</option>
                    <option value="06">6 期</option>
                    <option value="12">12 期</option>
                </select>
                <span class="requiredBlock">必填</span>
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
    /*var donatePurpose = lightBox.find('.donatePurpose');
    var otherBlock = lightBox.find('.otherBlock');
    donatePurpose.on('change', function () {
        if ($(this).val() == 15) {
            otherBlock.append(`
        <div class="fieldBlock">
            <input type="text" class="otherPurpose" id="otherPurpose" placeholder="自行填寫捐款用途">
        </div>
        `);
        } else {
            otherBlock.html('');
        }
    });*/
    // 選擇是否要收據
    let checkStatus = lightBox.find(".receipt");
    let addReceiptBlock = lightBox.find(".addReceiptBlock");
    checkStatus.on('change', function () {
        if ($(this).val() == "Y") {
            addReceiptBlock.html('').append(`
            <div class="fieldBlock">
                <label class="titleBlock" for="receiptTitle">收據抬頭</label>
                <input type="text" class="receiptTitle" id="receiptTitle">
            </div>
            <div class="fieldBlock radioBlock">
                <label for="deliveryMethod">是否寄送<span class="requiredBlock">必填</span></label>
                <div>
                    <div class="">
                    <input type="radio" id="deliveryMethod1" name="deliveryMethod" value="1" class="deliveryMethod" checked>
                    <label class="" for="deliveryMethod1">每次</label>
                    </div>
                    <div class="">
                    <input type="radio" id="deliveryMethod2" name="deliveryMethod" value="2" class="deliveryMethod">
                    <label class="" for="deliveryMethod2">月寄</label>
                    </div>
                    <div class="">
                    <input type="radio" id="deliveryMethod3" name="deliveryMethod" value="3" class="deliveryMethod">
                    <label class="" for="deliveryMethod3">年寄</label>
                    </div>
                    <div class="">
                    <input type="radio" id="deliveryMethod4" name="deliveryMethod" value="0" class="deliveryMethod">
                    <label class="" for="deliveryMethod4">否</label>
                    </div>
                </div>
            </div>
            `);
        } else {
            addReceiptBlock.html('');
        }
    });

    // lightBox 開啟預覽圖片
    var btnPreview = $('.btnPreview');
    var lightBoxImage = $('.lightBoxImageBlock');
    btnPreview.on('click', function () {
        var vImageSrc = $(this).find('img').attr('src');
        lightBoxImage.find('.lightBoxImage img').attr('src', vImageSrc);
        lightBoxImage.addClass('active');
        if (lightBoxImage.hasClass("active")) {
            $(document.body).css('overflow', 'hidden');
        } else {
            $(document.body).css('overflow', 'auto');
        }
        cancel = lightBoxImage.find('.cancel');
        cancel.on('click', function () {
            lightBoxImage.removeClass('active');
            $(document.body).css('overflow', 'auto');
        })
        lightBoxImage.on('click', function () {
            lightBoxImage.removeClass('active');
            $(document.body).css('overflow', 'auto');
        })
    });

    // ligthBox 開啟閱讀更多內容
    var lightBoxMore = $('.lightBoxMoreBlock');
    var btnMore = $('.btnMore');
    btnMore.on('click', function () {
        lightBoxMore.addClass('active');
        if (lightBoxMore.hasClass("active")) {
            $(document.body).css('overflow', 'hidden');
        } else {
            $(document.body).css('overflow', 'auto');
        }
        cancel = lightBoxMore.find('.cancel');
        cancel.on('click', function () {
            lightBoxMore.removeClass('active');
            $(document.body).css('overflow', 'auto');
        })
    });

    // relatedPage
    var relatedPage = $('.relatedPage');
    // relatedForms
    var relatedForms = relatedPage.find('.relatedForms');
    // relatedList 
    var relatedList = relatedPage.find('.relatedForms .relatedList');
    // relatedPicks 
    var relatedPicks = relatedPage.find('.relatedForms .relatedPicks');
    var shippingMethods = relatedPage.find('.relatedForms .shippingMethods');
    // relatedDonateMethods
    var relatedMethods = relatedPage.find('.donateMethods');
    var relatedStagingBlock = relatedPage.find('.stagingBlock');
    var quantities = relatedList.find('.quantities');
    var totalQuant = 0;
    var totalQuantities = $('.totalQuantities');
    var totalAmount = 0;
    var amounts = $('.amounts');
    var vTotal = 0;
    var totalCost = 0;
    var totals = $('.totals');
    var vFee = 0;
    var totalFee = 0;
    var fees = $('.fees');

    quantities.on('change', function () {
        // quantities 填寫結緣數量，帶出單筆總金額
        $(this).parent().parent().parent().find('.relatedGoods').each(function () {
            if ($(this).find('.quantities').val() == '') {
                $(this).find('.quantities').val('0');
            }
            // 計算各筆數價格
            var sinPrice = parseInt($(this).find('.sinPrice').val());
            var totalPrice = sinPrice * parseInt($(this).find('.quantities').val());
            $(this).find('.price').html('').append('$ ' + totalPrice);

            // 計算總共數量
            var quantity = parseInt($(this).find('.quantities').val());
            totalQuant += quantity;

            //計算總筆數金額
            totalAmount += totalPrice;
        });

        // 總共結緣品數量
        totalQuantities.html('').append(totalQuant);
        totalQuant = 0;
        // 計算結緣總金額
        amounts.html('').append('$ ' + totalAmount)
        totalCost += totalAmount;
        vTotal = totalCost;
        totalAmount = 0;
        totalCost += vFee;
        totals.html('').append(totalCost);
        if (totalCost > 20000) {
            relatedMethods.find('.storePay').attr('disabled', true);
        } else {
            relatedMethods.find('.storePay').attr('disabled', false);
        }
        totalCost = 0;
    });
    // shippingMethods 選擇運送方式，帶出填寫的欄位
    relatedPicks.on('change', function () {
        if (relatedPicks.val() == 1) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="shippingAddress">運送地址</label>
                <div class="addressBlock">
                    <div class="twZipCode"></div>
                    <input type="text" class="shippingAddress remark" placeholder="地址路段" id="shippingAddress">    
                </div>
            </div>
            `);
            $(".twZipCode").twzipcode({
                zipcodeIntoDistrict: true, // 郵遞區號自動顯示在地區
                css: ["addReceiptCity form-control", "addReceiptTown form-control"], // 自訂 "城市"、"地區" class 名稱 
                countyName: "city", // 自訂城市 select 標籤的 name 值
                districtName: "town" // 自訂地區 select 標籤的 name 值
            });
            totalFee = 100;
        }
        else if (relatedPicks.val() == 2) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="storeAddress">店到店方式</label>
                <input type="text" class="storeAddress remark" placeholder="請填寫超商地址或店號" id="storeAddress">    
            </div>
        `);
            totalFee = 60;
        }
        else if (relatedPicks.val() == 3) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="shippingNote">備註</label>
                <input type="text" class="shippingNote remark" placeholder="您有空的時間" id="shippingNote">    
            </div>
        `);
            totalFee = 0;
        }
        else if (relatedPicks.val() == 4) {
            shippingMethods.html('').append(`
            <div class="shippingMethod">
                <label class="titleBlock" for="shippingNote">備註</label>
                <input type="text" class="shippingNote remark" placeholder="分會地點及您有空的時間" id="shippingNote">    
            </div>
        `);
            totalFee = 0;
        }
        else {
            shippingMethods.html('');
            totalFee = 0;
        }
        // fees 選擇運送方式，帶出運費
        fees.html('').append('$ ' + totalFee);
        totalCost += totalFee;
        vFee = totalCost;
        totalFee = 0;

        totalCost += vTotal;
        totals.html('').append(totalCost);
        totalCost = 0;
    });
    relatedMethods.on('change', function () {
        // Staging 分期方式，如果付款方式選擇信用卡刷卡
        if ($(this).val() == 01) {
            relatedStagingBlock.html('').append(`
            <label for="stagingMethods">分期方式</label>
            <div>
                <select class="stagingMethods" id="stagingMethods">
                    <option value="01" selected>一次付清</option>
                    <option value="03">3 期</option>
                    <option value="06">6 期</option>
                    <option value="12">12 期</option>
                </select>
                <span class="requiredBlock">必填</span>
            </div>
        `).css('display', 'flex');
        } else {
            relatedStagingBlock.html('').css('display', 'none');
        }
    });
    // aboutPage.html
    // Tab 切換標籤
    var aboutPage = $('.aboutPage');
    var tab = aboutPage.find('.aboutTabs .tabItem');
    var aboutContent = aboutPage.find('.tabsContentsBlock .tabContent');
    tab.eq(0).addClass("active");
    aboutContent.eq(0).addClass("active");
    tab.on("click", function () {
        // 先移除所有Tab標籤的-on class，在對點擊的Tab標籤加上-on class
        $(this).parent().children().removeClass("active");
        $(this).addClass("active");

        aboutContent.removeClass("active");
        aboutContent.eq($(this).index()).addClass("active");
    });
    // showRules
    var rulesItem = aboutPage.find('.rulesItem');
    rulesItem.on('click', function () {
        $(this).find('>ul').slideToggle('normal');
    });

    // count the number of people
    var partition = aboutPage.find('.memberTables .partition');
    var nums = partition.find('.nums');
    nums.each(function () {
        var counts = $(this).parent().parent().parent().parent().find('.partContent');
        $(this).text(counts.length);
    });

    // servicePage.html
    // Tab 切換標籤
    var servicePage = $('.servicePage');
    var tab = servicePage.find('.serviceTabs .tabItem');
    var serviceContent = servicePage.find('.tabsContentsBlock .tabContent');
    tab.eq(0).addClass("active");
    serviceContent.eq(0).addClass("active");
    tab.on("click", function () {
        // 先移除所有Tab標籤的-on class，在對點擊的Tab標籤加上-on class
        $(this).parent().children().removeClass("active");
        $(this).addClass("active");

        serviceContent.removeClass("active");
        serviceContent.eq($(this).index()).addClass("active");
    });

    // videoPage.html
    // slider 顯示/隱藏完整內容
    var videoPage = $('.videoPage');
    var btnShowFull = videoPage.find('.btnShowFull');
    var tab = videoPage.find('.videoTabs .tabItem');
    tab.eq(0).addClass("active");
    tab.on('click', function () {
        // 先移除所有Tab標籤的-on class，在對點擊的Tab標籤加上-on class
        $(this).parent().children().removeClass("active");
        $(this).addClass("active");
    });
    var videoPlayBlock = videoPage.find('.videoPlayBlock');
    var videoPlaysContents = videoPage.find('.videoPlaysContents');
    btnShowFull.on('click', function () {
        $(this).toggleClass('active');
        videoPlaysContents.slideToggle('normal');
    });
    var videoPlaying = videoPage.find('.videoPlayBlock');
    //
    var videoItem = videoPage.find('.videoPendingList ul li');
    var videos;
    var destination = videoPage.find('.videoTabsBlock');
    videoItem.on('click', function () {
        $(this).addClass('active').siblings().removeClass();
        videos = $(this).find('iframe').attr('src');
        videoPlayBlock.find('.videoPlays iframe').attr('src', videos);
        videoPlayBlock.find('.videoPlaysTitleBlock .mainTitle').text($(this).find('.titleBlock .mainTitle').text());
        videoPlayBlock.find('.videoPlaysContents p:nth-child(1)').text($(this).find('.contentText p:nth-child(1)').text());
        videoPlayBlock.find('.videoPlaysContents p:nth-child(2)').text($(this).find('.contentText p:nth-child(2)').text());

        $('html,body').animate({
            scrollTop: destination.offset().top
        }, 0);
    });
    // videoItem.eq(0).trigger('click');
    // 單純addClass並無其它作用，一開始需載入第一筆的內容
    videoItem.eq(0).addClass('active');

    // eventsPage.html
    var eventsPage = $('.eventsPage');
    var tab = eventsPage.find('.eventsTabs .tabItem');
    tab.on('click', function () {
        // 先移除所有Tab標籤的-on class，在對點擊的Tab標籤加上-on class
        $(this).parent().children().removeClass("active");
        $(this).addClass("active");
    });

    //
    //var insidePage = $('.insidePage');
    //var contentVideo = $('.contentVideo');
    //contentVideo.hide();
    //contentVideo.each(function () {
    //    $(this).has('iframe').show();
    //    $(this).find('iframe').removeAttr('width height');
    //});
    // contactPage.html 
    var contactPage = $('.contactPage');
    var contactForm = contactPage.find('.contactForm');
    var c = {}, check = true, errorTxt = '';
    // 正規表示法驗證
    c.re_name = /^[\u4e00-\u9fa5]{2,5}$/; //中文姓名2-5字
    c.re_cel = /^[09]{2}\d{8}$/; //手機號碼
    c.re_eml = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z]+$/; //電子信箱
    function checkForm() {
        var name = contactForm.find('#contactName'),
            phone = contactForm.find('#contactTel'),
            email = contactForm.find('#contactMail'),
            content = contactForm.find('#contactContent');

        if (name.val() == '') {
            errorTxt = "請您填寫真實姓名！";
            check = false;
            return check;
        }
        if (phone.val() == '') {
            errorTxt = "請您填寫手機號碼！";
            check = false;
            return check;
        } else if (!c.re_cel.test(phone.val())) {
            errorTxt = "請填寫正確的手機格式09XX000000(共10碼)！";
            check = false;
            return check;
        }
        /*if (email.val() == '') {
            errorTxt = "請您填寫 E-mail！";
            check = false;
            return;*/
        if (email.val().length > 0 && !c.re_eml.test(email.val())) {
            errorTxt = "請填寫正確的 E-mail 格式！";
            check = false;
            return check;
        }
        if (content.val() == '') {
            errorTxt = "請您填寫您想詢問的內容！";
            check = false;
            return check;
        }
        return check;
    }
    // 送出submit 和 重置reset
    var btnSubmit = contactForm.find('.btnSubmit');
    var btnReset = contactForm.find('.btnReset');
    btnSubmit.click(() => {
        checkForm();
        if (check) {
            var postData = new FormData($("#fmContact")[0]);
            var xhr = new XMLHttpRequest();
            //xhr.open("POST", "@Url.Action("SendMailToBranch", "Act")", true);
            xhr.open("POST", "Act/SendMailToBranch", true);
            xhr.onload = function () {
                if (this.status === 200 && this.responseText === "OK") {
                    var branch = $("#fmContact select option:checked").text();
                    Swal.fire(`已經成功將聯絡內容發送給 ${branch}`).then(() => {
                        $("#fmContact .btnReset").click();
                    });
                }
            };
            xhr.send(postData);
        } else {
            Swal.fire(errorTxt);
        }
    });
    btnReset.click(() => {
        $('#fmContact')[0].reset();
    });
    /*btnSubmit.click(function () {
        checkForm();
        if (check == true) {
            if (confirm("確定要傳送表單嗎？")) {
                contactForm.get(0).submit();
            }
        } else {
            alert(errorTxt);
        }
    });*/
    $(window).scroll(function () {
        var bwsHeight = $(document).scrollTop();
        if (bwsHeight >= 576) {
            contactForm.addClass('active');
        } else {
            contactForm.removeClass('active')
        }
    });

    // contactPage.html relatedPage.html
    // 取下 GooleMap 自帶的寬高屬性
    var contactItem = $('.contactItem');
    contactItem.each(function () {
        $(this).find('.mapBlock iframe').removeAttr('width height');
    });

    // Footer Subscribe to newsletter
    var footer = $('.footer');
    var btnOpen = footer.find('.contentFooter .btnOpen');
    var messageBox = footer.find('.messageBoxBlock');
    btnOpen.on('click', function (e) {
        messageBox.addClass('active');

        cancel = messageBox.find('.cancel');
        cancel.on('click', function () {
            messageBox.removeClass('active');
        });
    });

    // 點擊除了燈箱以外的地方，關閉燈箱
    //$(document).mouseup(function (e) {
    //    if (!messageBox.is(e.target) && messageBox.has(e.target).length === 0) {
    //        messageBox.removeClass('active');
    //    }
    //});

    // btnClose 
    var guidePage = $('.guidePage');
    var btnClose = guidePage.find('.btnClose');
    btnClose.on('click', function () {
        window.opener = null;
        window.close();
    });

    $('.receipt:checked').change();
    $('.donateMethods').change();

    $(".btnSub").click(() => {
        var substr = $("input[name=subMmail]").val();
        if (substr.length === 0) {
            Swal.fire("請填寫 E-mail！");
        }
        else if (substr.length > 0 && !c.re_eml.test(substr)) {
            Swal.fire("請填寫正確的 E-mail 格式！");
        }
        else {
            var postData = new FormData();
            postData.append("subMmail", $("#subMmail").val());
            var xhr = new XMLHttpRequest();
            xhr.open("POST", "/Act/AddSubscription", true);
            xhr.onload = function () {
                if (this.status === 200 && this.responseText === "OK") {
                    Swal.fire("感謝您的訂閱！");
                } else if (this.status === 200 && this.responseText === "EX") {
                    Swal.fire("您已經訂閱過了，感謝您的訂閱！");
                }
            };
            xhr.send(postData);
        }
    })

});
