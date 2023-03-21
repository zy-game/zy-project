function request(apiName, func) {
    window.$.ajax({
        headers: {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "PUT,POST,GET,DELETE,OPTIONS"
        },
        contentType: "application/text",
        type: "GET",
        url: window.url + apiName,
        success: function (data) {
            func(data)
        }
    })
}

function request2(apiName, data, func) {
    window.$.ajax({
        headers: {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "PUT,POST,GET,DELETE,OPTIONS"
        },
        contentType: "application/text",
        type: "GET",
        url: window.url + apiName + "/" + data,
        success: function (data) {
            func(data)
        }
    })
}

function getTimelineItemString(item) {
    let result = '<li class="layui-timeline-item">'
    result += '<i class="layui-icon layui-timeline-axis"></i>'
    result += '<div class="layui-timeline-content layui-text">'
    result += '<a id="' + item.id + ' href="javascript:;" οnclick="onInfo(' + item.id + ')">'
    result += '<h3 class="layui-timeline-title">' + item.title + '</h3>'
    result += '</a>'
    result += '<p>' + item.simple + '</p>'
    result += '<ul>'
    result += '</ul>'
    result += '</div>'
    result += '</li>'
    return result
}
