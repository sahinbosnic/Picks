var cdnUrl = ""; 
$(document).ready(function () {

    $.ajax({
        type: 'GET',
        url: '/home/GetCdnUrl',
        success: function (result) {
            if (result.length > 0)
            {
                cdnUrl = result
            }
            
            
        }
    }).then(function () {
        GetAllImages()
        GetAllTags()
    })
    //GetAllImages()
    //GetAllTags()
    $('.search-box').select2()
})

$(document).on("click", ".addToBasket", function () {
    var imgUrl = $(this).closest(".image-box").children("img").attr("src")
   
    imgUrl = imgUrl.replace("/uploads/", "")
    $('.imageList').append(
        ['<span class="col-12 mt-2 cart-item">' + imgUrl,
            '<div class="float-right">',
            '<a href="/uploads/' + imgUrl + '" class="cart-url btn btn-info mr-2" download><i class="fa fa-cloud-download" aria-hidden="true"></i></a>',
            '<span class="btn btn-danger removeCartItem"><i class="fa fa-times" aria-hidden="true"></i></span>',
            '</div>',
            '</span>'
        ].join("")
    )

})

$(document).on("click", ".removeCartItem", function () {
    $(this).closest(".cart-item").remove()
})


$("#addImage form").submit(function (e) {
    e.preventDefault()

    if ($("#addImage form input[type='file']")[0].files.length > 0) {
        var formData = new FormData()
        $.each($("input[type='file']")[0].files, function (i, file) {
            formData.append('files', file)
        })
        var tags = $("#imageTags").val()
        formData.append('tags', tags)

        $.ajax({
            type: 'POST',
            url: '/home/upload',
            cache: false,
            contentType: false,
            processData: false,
            data: formData,
            success: function (result) {
                $(".closeUpload").click()
                GetAllImages()
                GetAllTags()
            },
            error: function (err) {
                $("#errorMessage").text("Something went wrong with the upload, make sure you upload images only!")
                $("#errorMessage").show()
            }
        })
    }
})

$("#generateZip").click(function () {
    $("#downloadAll").hide()
    var imageList = []
    $(".cart-item").each(function () {
        imageList.push($(this).text())
    })

    $.ajax({
        type: 'post',
        url: '/home/getZipUrl',
        data: { images: imageList },
        success: function (result) {
            $("#downloadAll").attr("href", '/zip/'+result)
            $("#downloadAll").show()

        },
    })
})

function GetAllTags() {
    $.ajax({
        type: 'GET',
        url: '/home/gettags',
        success: function (result) {
            $('.search-box').empty()
            $.each(result, function (i, tag) {
                //console.log(i, tag)
                $('.search-box').append(new Option(tag, tag))
            })
        },
    })
}

$('.search-box').on("change", function () {
    var searchArr = $(this).val()
    GetAllImages(searchArr)
})

function GetAllImages(searchArr)
{
    $("#imageContainer").empty()
    $.ajax({
        type: 'Post',
        url: '/home/GetImages',
        data: { search: searchArr },
        success: function (result) {
            $.each(result, function (i, file) {
                console.log("cdnUrl", cdnUrl)
                var rend = [
                    '<div class="image-box col-md-3 mt-4">',
                    '<img src="' + cdnUrl +'/uploads/' + file.FileName + '">',
                    '<div>',
                    '<span class="btn btn-success addToBasket"> <i class="fa fa-shopping-basket" aria-hidden="true"></i></span>',
                    '<a href="/uploads/' + file.FileName + '" class="btn btn-info" download><i class="fa fa-cloud-download" aria-hidden="true"></i></a>',
                    '</div > ',
                    '</div>'

                ].join("")

                $("#imageContainer").append(rend)
            })

            $('.search-box').trigger("reset")
            $("#uploadForm").trigger("reset")

            
        },
    })
}

