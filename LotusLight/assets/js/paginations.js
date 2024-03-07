// //  純前端產生分頁器
function pageController(iNow, mainContent, perDis, count) {
    var p_items = mainContent.find('.pageContents li');

    var pages = Math.ceil(p_items.length / perDis);
    var count = count;
    var nextBlock = mainContent.find('.pagination .nextBlock');
    var preBlock = mainContent.find('.pagination .preBlock');
    var pagination = mainContent.find('.pagination ul');
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
        // pagination.find(`li:contains(${currentPage})`).addClass('active').siblings().removeClass('active');
        if (currentPage * perDis > p_items.length) {
            showItems = p_items.slice((currentPage - 1) * perDis);
        }
        else {
            showItems = p_items.slice((currentPage - 1) * perDis, currentPage * perDis);
        }
        p_items.removeClass('active');
        showItems.addClass('active');
        $('html,body').animate({
            scrollTop: $('.eventsTitleBlock').offset().top - 30
        }, 0);
        $('.btnNext').on('click', function (e) {
            e.preventDefault();
            currentPage++;
            pagination.find(`li:contains(${currentPage})`).trigger('click');
        })
        $('.btnPre').on('click', function (e) {
            e.preventDefault();
            currentPage--;
            pagination.find(`li:contains(${currentPage})`).trigger('click');
        })
    });

    pagination.find('li:first-child a').text(iNow).trigger('click');
    /*pagination.find('li a').each((idx, obj) => {
        if ($(obj).text() == String(iNow)) {
            $(obj).trigger('click');
            return;
        }
    });*/
};
var newsPage = $('.newsPage').find('.pageControllers');
pageController(1, newsPage, 5, 2);

var eventPage = $('.eventPage').find('.pageControllers');
pageController(1, eventPage, 3, 2);
