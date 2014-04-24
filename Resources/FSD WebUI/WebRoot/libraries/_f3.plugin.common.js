f3.extend("classes").ratingdescriptor = function () {
    this.ratingdescriptorid = "";
    this.ratingdescriptorlevel = ""
};
f3.extend("classes").gamecapabilities = function () {
    this.offlineplayersmin = "";
    this.offlineplayersmax = "";
    this.offlinecoopplayersmin = "";
    this.offlinecoopplayersmax = "";
    this.offlinesystemlinkmin = "";
    this.offlinesystemlinkmax = "";
    this.offlinemaxhdtvmodeid = "";
    this.offlinedolbydigital = "";
    this.onlinemultiplayermin = "";
    this.onlinemultiplayermax = "";
    this.onlinecoopplayersmin = "";
    this.onlinecoopplayersmax = "";
    this.onlinecontentdownload = "";
    this.onlineleaderboards = "";
    this.onlinevoice = ""
};
f3.extend("classes").category = function () {
    this.categoryid = "";
    this.system = "";
    this.name = ""
};
f3.extend("classes").video = function () {
    this.videodefinition = "";
    this.videoencoding = "";
    this.audioencoding = "";
    this.isacquirable = "";
    this.aspectratio = "";
    this.resolution = "";
    this.url = ""
};
f3.extend("classes").boxart = function () {
    this.small = "";
    this.large = ""
};
f3.extend("classes").images = function () {
    this.icon = "";
    this.banner = "";
    this.boxart = new f3.classes.boxart;
    this.background = "";
    this.screenshots = []
};
f3.extend("classes").liveinfo = function () {
    this.parsed = false;
    this.mediatype = "";
    this.gametitlemediaid = "";
    this.reducedtitle = "";
    this.reduceddescription = "";
    this.availabilitydate = "";
    this.releasedate = "";
    this.ratingid = "";
    this.developer = "";
    this.publisher = "";
    this.newestofferstartdate = "";
    this.totaloffercount = "";
    this.totalsubscriptioncount = "";
    this.titleid = "";
    this.effectivetitleid = "";
    this.gamereducedtitle = "";
    this.fulltitle = "";
    this.description = "";
    this.ratingaggregate = "";
    this.numberofratings = "";
    this.categories = [];
    this.ratingdescriptors = [];
    this.video = new f3.classes.video;
    this.images = new f3.classes.images;
    this.gamecapabilities = new f3.classes.gamecapabilities
};
f3.extend("classes").userdata = function () {
    var _classversion = "1";
    var _objectversion = "0";
    var _settings = {};
    this.version = null;
    this.settings = {};
    Object.defineProperty(this, "version", {
        get: function () {
            return _classversion
        },
        set: function (version) {
            _objectversion = version
        }
    });
    Object.defineProperty(this, "settings", {
        get: function () {
            return _settings
        },
        set: function (settings) {
            $.extend(true, _settings, settings)
        }
    });
    this.load = function (settings) {
        var actionurl = "getUserData";
        var t = this;
        if (typeof settings == "undefined") settings = {};
        if (typeof settings.verbose == "undefined") settings.verbose = true;
        $.ajax({
            type: "get",
            cache: false,
            dataType: "json",
            url: actionurl,
            success: function (json, textStatus, jqXHR) {
                $.each(json, function (name, value) {
                    t[name] = value
                });
                if ($.isFunction(settings.success)) settings.success(t, json, textStatus, jqXHR)
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (settings.verbose === true) f3.showmessage("Error loading UserData: " + jqXHR.status + " " + errorThrown);
                if ($.isFunction(settings.error)) settings.error(t, jqXHR, textStatus, errorThrown)
            },
            complete: function (jqXHR, textStatus) {
                if ($.isFunction(settings.complete)) settings.complete(t, jqXHR, textStatus)
            }
        })
    };
    this.save = function (settings) {
        var actionurl = "postUserData";
        var t = this;
        if (typeof settings == "undefined") settings = {};
        if (typeof settings.verbose == "undefined") settings.verbose = true;
        $.ajax({
            type: "post",
            cache: false,
            dataType: "text",
            url: actionurl,
            data: {
                userdata: JSON.stringify(t)
            },
            success: function (json, textStatus, jqXHR) {
                if ($.isFunction(settings.success)) settings.success(t, json, textStatus, jqXHR)
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (settings.verbose === true) f3.showmessage("Error saving UserData: " + jqXHR.status + " " + errorThrown);
                if ($.isFunction(settings.error)) settings.error(t, jqXHR, textStatus, errorThrown)
            },
            complete: function (jqXHR, textStatus) {
                f3.groups.userdata("userdata", true);
                if ($.isFunction(settings.complete)) settings.complete(t, jqXHR, textStatus)
            }
        })
    };
    this.notes = {}
};
f3.extend("XML").settings = {
    liveinfo: [{
        rootpath: ["feed", "entry", "media"],
        namespace: "live"
    }, {
        rootpath: ["feed", "entry", "categories", "category"],
        mapping: {
            categories: f3.classes.category
        },
        namespace: "live"
    }],
    "liveinfo.gamecapabilities": [{
        rootpath: ["feed", "entry", "media", "gamecapabilities"],
        namespace: "live"
    }],
    "liveinfo.categories.categories": [{
        rootpath: [],
        namespace: "live"
    }],
    "liveinfo.images": [{
        rootpath: ["feed", "entry", "images", "image"],
        mapping: {
            icon: 'fileUrl:contains("tile.png")',
            banner: 'fileUrl:contains("banner.png")',
            background: 'fileUrl:contains("background.jpg")',
            boxart: null
        },
        namespace: "live"
    }, {
        rootpath: ["feed", "entry", "slideshows", "slideshow", "image"],
        mapping: {
            screenshots: "fileUrl"
        },
        namespace: "live"
    }],
    "liveinfo.images.boxart": [{
        rootpath: ["feed", "entry", "images", "image"],
        mapping: {
            small: 'fileUrl:contains("boxartsm.jpg")',
            large: 'fileUrl:contains("boxartlg.jpg")'
        },
        namespace: "live"
    }],
    "liveinfo.video": [{
        rootpath: ["feed", "entry", "previewinstances", "videopreviewinstance"],
        namespace: "live"
    }, {
        rootpath: ["feed", "entry", "previewinstances", "videopreviewinstance", "files", "file"],
        mapping: {
            url: "fileUrl"
        },
        namespace: "live"
    }]
};
f3.extend("XML").parse = function (object, xml, settingsname) {
    if (typeof xml === "undefined" || xml === null) return false;
    var parsed = true;
    var settings = f3.XML.settings[settingsname];
    if (typeof settings === "undefined") return false;
    $.each(settings, function (name, value) {
        var settings = value;
        var path = settings.rootpath;
        var namespace = settings.namespace;
        var usemapping = !(typeof settings.mapping === "undefined" || $.isEmptyObject(settings.mapping));
        $.each(usemapping ? settings.mapping : object, function (name, value) {
            if (typeof f3.XML.settings[settingsname + "." + name] !== "undefined") {
                parsed = f3.XML.parse(object[name], xml, settingsname + "." + name) === false ? false : parsed
            } else {
                var isobject = usemapping && $.isFunction(value);
                if ($.isArray(object[name])) {
                    $(f3.XML.getnode(xml, isobject ? path : path.concat(usemapping ? value : name), namespace)).each(function () {
                        if (isobject) {
                            var tmp = new value;
                            parsed = f3.XML.parse(tmp, this, settingsname + "." + name + "." + name) === false ? false : parsed;
                            object[name].push(tmp)
                        } else {
                            object[name].push($(this).text())
                        }
                    })
                } else {
                    if (isobject) {
                        var tmp = new value;
                        var result = $(f3.XML.getnode(xml, path.concat(usemapping ? value : name), namespace));
                        parsed = f3.XML.parse(tmp, this, settingsname + "." + name + "." + name) === false ? false : parsed;
                        object[name] = tmp
                    } else {
                        object[name] = $(f3.XML.getnode(xml, path.concat(usemapping ? value : name), namespace)).text()
                    }
                }
            }
        })
    });
    if (typeof object.parsed !== "undefined") object.parsed = parsed;
    return object
};
f3.extend("XML").getnode = function (xml, path, namespace, eager) {
    var node = xml;
    var childonly = eager === true ? "" : "> ";
    $(path).each(function () {
        node = $(node).find(childonly + this + ", " + childonly + namespace + "\\:" + this)
    });
    return node
};
f3.extend("common").getvalue = function (object, name) {
    if (typeof name !== "string" || name === "") return;
    var parts = name.split(".");
    var parent = object;
    for (var i = 0; i < parts.length; i++) {
        if (typeof parent[parts[i]] === "undefined") return;
        parent = parent[parts[i]]
    }
    return parent
};
f3.extend("common").setvalue = function (object, name, value) {
    if (typeof name !== "string" || name === "") return object;
    var parts = name.split(".");
    var parent = object;
    var current = parent;
    for (var i = 0; i < parts.length; i++) {
        parent = current;
        if (typeof parent[parts[i]] === "undefined") {
            parent[parts[i]] = {}
        }
        current = parent[parts[i]]
    }
    if (typeof current !== "undefined" && typeof parent !== "undefined") {
        parent[parts.slice(-1)] = value
    }
    return object
};
f3.extend("common.defines").validgamegenres = ["0000", "0001", "0002", "0003", "3001", "3002", "3005", "3006", "3007", "3008", "3009", "3010", "3011", "3012", "3013", "3018", "3019", "3022"];
f3.extend("common.defines").drivemounts = {
    "/SystemRoot": "Flash:",
    "/Device/BuiltInMuUsb/Storage": "OnBoardMU:",
    "/Device/BuiltInMuMmc/Storage": "OnBoardMU:",
    "/Device/BuiltInMuSfc": "OnBoardMU:",
    "/Device/Cdrom0": "Dvd:",
    "/Device/Harddisk0/Partition1": "Hdd1:",
    "/Device/Harddisk0/Partition0": "Hdd0:",
    "/Device/Harddisk0/SystemPartition": "HddX:",
    "/sep": "SysExt:",
    "/Device/Mu0": "Memunit0:",
    "/Device/Mu1": "Memunit1:",
    "/Device/Mass0": "Usb0:",
    "/Device/Mass1": "Usb1:",
    "/Device/Mass2": "Usb2:",
    "/Device/HdDvdPlayer": "HdDvdPlayer:",
    "/Device/HdDvdStorage": "HdDvdStorage:",
    "/Device/Transfercable": "Transfercable:",
    "/Device/Transfercable/Compatibility/Xbox1": "TransfercableXbox1:",
    "/Device/Mass0PartitionFile/Storage": "USBMU0:",
    "/Device/Mass1PartitionFile/Storage": "USBMU1:",
    "/Device/Mass2PartitionFile/Storage": "USBMU2:",
    "/Device/Mass0PartitionFile/StorageSystem": "USBMUCache0:",
    "/Device/Mass1PartitionFile/StorageSystem": "USBMUCache1:",
    "/Device/Mass2PartitionFile/StorageSystem": "USBMUCache2:",
    "\\SystemRoot": "Flash:",
    "\\Device\\BuiltInMuUsb\\Storage": "OnBoardMU:",
    "\\Device\\BuiltInMuMmc\\Storage": "OnBoardMU:",
    "\\Device\\BuiltInMuSfc": "OnBoardMU:",
    "\\Device\\Cdrom0": "Dvd:",
    "\\Device\\Harddisk0\\Partition1": "Hdd1:",
    "\\Device\\Harddisk0\\Partition0": "Hdd0:",
    "\\Device\\Harddisk0\\SystemPartition": "HddX:",
    "\\sep": "SysExt:",
    "\\Device\\Mu0": "Memunit0:",
    "\\Device\\Mu1": "Memunit1:",
    "\\Device\\Mass0": "Usb0:",
    "\\Device\\Mass1": "Usb1:",
    "\\Device\\Mass2": "Usb2:",
    "\\Device\\HdDvdPlayer": "HdDvdPlayer:",
    "\\Device\\HdDvdStorage": "HdDvdStorage:",
    "\\Device\\Transfercable": "Transfercable:",
    "\\Device\\Transfercable\\Compatibility\\Xbox1": "TransfercableXbox1:",
    "\\Device\\Mass0PartitionFile\\Storage": "USBMU0:",
    "\\Device\\Mass1PartitionFile\\Storage": "USBMU1:",
    "\\Device\\Mass2PartitionFile\\Storage": "USBMU2:",
    "\\Device\\Mass0PartitionFile\\StorageSystem": "USBMUCache0:",
    "\\Device\\Mass1PartitionFile\\StorageSystem": "USBMUCache1:",
    "\\Device\\Mass2PartitionFile\\StorageSystem": "USBMUCache2:"
};
f3.extend("common.defines").versiontypes = ["undefined", "ALPHA", "BETA", "RELEASE"];
f3.extend("common.defines").features = {
    multidisc: "Multi Disc",
    trainers: "Trainers",
    systemlink: "System Link",
    gamepad: "Game Pad",
    achievements: "Achievements",
    threads: "Game Threads",
    httpdaemon: "Web Server",
    "debugger": "Debugger",
    network: "Network"
};
f3.extend("common.defines").achievementtypes = {
    1: "Completion",
    2: "Leveling",
    3: "Unlock",
    4: "Event",
    5: "Tournament",
    6: "Checkpoint",
    7: "Other"
};
f3.extend("common.defines.launchdata").exectype = {
    none: -1,
    xex: 0,
    xbe: 1,
    xexcon: 2,
    xbecon: 3,
    xnacon: 4
};
f3.extend("behavior").trigger = function (object, value, event, data, image) {
    var search = [$(object).data("name"), typeof $(object).data("type") !== "undefined" ? $(object).data("type") : "text", event];
    var found = f3.behavior.events;
    for (var i = 0; typeof found !== "undefined" & i < search.length; i++) {
        found = found[search[i]]
    }
    if (typeof found === "function") {
        return found(object, value, data, image)
    }
    return value
};
f3.extend("behavior").process = function (object, value, data) {
    var search = [$(object).data("name"), typeof $(object).data("condition") !== "undefined" ? $(object).data("condition") : ""];
    var found = f3.behavior.conditions;
    for (var i = 0; typeof found !== "undefined" & i < search.length; i++) {
        found = found[search[i]]
    }
    if (typeof found === "function") {
        found(object, value, data)
    }
};
f3.extend("data").parse = function (data, group, context) {
    if (typeof context === "undefined") context = document;
    var filter = "[data-name]:not([data-condition])" + (typeof group !== "undefined" ? '[data-group="' + group + '"]' : ":not([data-group])");
    $(context).find(filter).addBack(filter).each(function () {
        var value = f3.common.getvalue(data, $(this).data("name"));
        if (typeof value === "undefined") return;
        if (typeof $(this).data("html") === "undefined") $(this).data("html", $(this).html());
        switch ($(this).data("type")) {
        case "attribute":
            value = f3.behavior.trigger(this, value, "onBeforeSetValue", data);
            $(this).attr($(this).data("attribute"), value);
            value = f3.behavior.trigger(this, value, "onAfterSetValue", data);
            break;
        case "html":
            value = f3.behavior.trigger(this, value, "onBeforeSetValue", data);
            if ($(this).html() !== value) $(this).html(value);
            value = f3.behavior.trigger(this, value, "onAfterSetValue", data);
            break;
        case "image":
        case "background":
            var tmpimage = new Image;
            var t = this;
            $(tmpimage).one("error", function () {
                var result = f3.behavior.trigger(t, value, "onLoadFailed", data, tmpimage);
                f3.log("imgerror", t, result)
            });
            $(tmpimage).one("load", function () {
                var result = "";
                switch ($(t).data("type")) {
                case "image":
                    if ($(t).prop("tagName") === "IMG") {
                        $(t).load(function () {
                            result = f3.behavior.trigger(t, value, "onLoadSucceeded", data, tmpimage)
                        });
                        $(t).attr("src", "");
                        $(t).attr("src", tmpimage.src)
                    } else {
                        $(t).html(tmpimage);
                        result = f3.behavior.trigger(t, value, "onLoadSucceeded", data, tmpimage)
                    }
                    break;
                case "background":
                    $(t).css("background-image", "url('" + tmpimage.src + "')");
                    result = f3.behavior.trigger(t, value, "onLoadSucceeded", data, tmpimage);
                    break
                }
                f3.log("imgload", t, result)
            });
            value = f3.behavior.trigger(this, value, "onBeforeSetValue", data, tmpimage);
            tmpimage.src = value;
            f3.behavior.trigger(this, value, "onAfterSetValue", data, tmpimage);
            break;
        case "date":
            var format = $(this).data("format");
            value = f3.behavior.trigger(this, value, "onBeforeFormatValue", data);
            value = value === "" ? value : $.format.date(value, typeof format !== "undefined" ? format : "yyyy/MM/dd");
            f3.behavior.trigger(this, value, "onAfterFormatValue", data);
        case "text":
        default:
            value = f3.behavior.trigger(this, value, "onBeforeSetValue", data);
            $(this).text(value);
            f3.behavior.trigger(this, value, "onAfterSetValue", data);
            break
        }
    });
    filter = "[data-name][data-condition]" + (typeof group !== "undefined" ? '[data-group="' + group + '"]' : ":not([data-group])");
    $(context).find(filter).each(function () {
        var value = f3.common.getvalue(data, $(this).data("name"));
        if (typeof value === "undefined") return;
        if (typeof $(this).data("html") === "undefined") $(this).data("html", $(this).html());
        f3.behavior.process(this, value)
    })
};
f3.bindall = function () {
    $("[data-refresh-once]").click(function () {
        var value = f3.behavior.trigger($(this), $(this).data("refresh-once"), "onBeforeRefreshOnce") + 1;
        f3.log("Refresh once", $(this).data("refresh-once"));
        var group = f3.groups[$(this).data("refresh-once")];
        if (typeof group !== "function") return;
        var t = this;
        setTimeout(function () {
            group($(t).data("refresh-once"), true);
            f3.behavior.trigger($(t), $(t).data("refresh-Once"), "onAfterRefreshOnce")
        }, value)
    });
    $("[data-refresh-toggle]").click(function () {
        var value = f3.behavior.trigger($(this), $(this).data("refresh-toggle"), "onBeforeRefreshToggle");
        var index = f3.settings.timer.groups.indexOf(value);
        f3.log("Refresh toggle", $(value), value, index < 0 ? "off -> on" : "on -> off");
        if (index < 0) {
            f3.settings.timer.groups.push(value);
            $(this).data("refresh-current", "on")
        } else {
            f3.settings.timer.groups.splice(index, 1);
            $(this).data("refresh-current", "off")
        }
        f3.behavior.trigger($(this), $(this).data("refresh-toggle"), "onAfterRefreshToggle")
    });
    $("[data-hide]").click(function () {
        var value = f3.behavior.trigger($(this), $(this).data("hide"), "onBeforeHide");
        $(value).fadeOut(400, function () {
            f3.behavior.trigger($(this), $(this).data("hide"), "onAfterHide")
        })
    });

    function addlog(node, names) {
        var name = names.slice(-1)[0];
        if (typeof name === " undefined" || typeof node[name] === "undefined") return;
        if (typeof node[name] === "function") {
            node[name] = new Function("object, value", "f3.log('(" + names + ")', object, value); return " + node[name].toString() + "(object, value);")
        } else {
            $.each(node[name], function (n, v) {
                addlog(node[name], names.concat(n))
            })
        }
    }
    if (f3.parameters.debug == 1) {
        addlog(f3.behavior, ["events"]);
        addlog(f3.behavior, ["conditions"])
    }

    function bindhotkey(key, object, tab) {
        $(window).keypress(function (eventObject) {
            if ($(eventObject.target).is("input, textarea")) return;
            if ($(eventObject.target).is(".fancybox-lock") || $(eventObject.target).parents(".fancybox-lock").size() > 0) return;
            if (String.fromCharCode(eventObject.which).toLowerCase() === key) {
                btn_click($("#btn_" + tab).get(0), $(object).click().position().top - $(object).parents("li.f3.content").position().top - 25)
            }
        })
    }
    bindhotkey("s", $('[data-refresh-once="screencapture"]'), "notes");
    bindhotkey("p", $('[data-suspend="1"]'), "system");
    bindhotkey("r", $('[data-suspend="0"]'), "system")
};
f3.extend("settings.timer").interval = 3e3;
f3.extend("settings.timer").groups = [];