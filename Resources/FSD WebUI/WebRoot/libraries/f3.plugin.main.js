/// <reference path="f3.plugin.common.js" />

//create a namespace f3
var f3 =
{
    extend: function(namespaces)
    {
        if (typeof namespaces !== 'string' || namespaces === '') return false;
        var parts = namespaces.split('.');
        var parent = this;

        if (typeof window[parts[0]] !== 'undefined' && window[parts[0]] === this)
        {
            parts = parts.slice(1);
        }

        for (var i = 0; i < parts.length; i++)
        {
            //create a property if it doesnt exist  
            if (typeof parent[parts[i]] === 'undefined')
            {
                parent[parts[i]] = {};
            }
            parent = parent[parts[i]];
        }

        return parent;
    },

    parameters: function()
    {
        var params = {};
        window.location.search.replace(/([^?=&]+)(=([^&]*))?/g, function(full, grp1, grp2, grp3)
        {
            params[grp1] = grp3;
        });
        return params;
    } (),

    log: function()
    {
        if (typeof f3.parameters.debug === 'undefined' || f3.parameters.debug != 1) return;

        var s = "";
        for (var i in arguments)
        {
            s += ', arguments[' + i + ']';
        }
        eval('console.log(\'f3.log: [\'' + s + ', \']\');');
    },

    data: {},

    groups:
    {
        liveinfo: function(group, refresh)
        {
            var actionurl = '';

            if (refresh === true)
            {
                // if refreshing, get the XML instead of the cached version
                actionurl = 'getGameXML';

                $.ajax(
                {
                    type: 'get',
                    cache: false,
                    dataType: 'xml',
                    url: actionurl,
                    success: function(xml, textStatus, jqXHR)
                    {
                        //if the XML is retreived, parse it and cache the parsed LiveInfo
                        f3.data.liveinfo = new f3.classes.liveinfo();
                        f3.XML.parse(f3.data.liveinfo, xml, group);
                        f3.data.parse(f3.data, group);

                        var actionurl = 'postLiveInfo';

                        $.ajax(
                        {
                            type: 'post',
                            cache: false,
                            dataType: 'text',
                            url: actionurl,
                            data: { "liveinfo": JSON.stringify(f3.data.liveinfo) },
                            error: function(jqXHR, textStatus, errorThrown)
                            {
                                f3.showmessage('Error saving LiveInfo: ' + jqXHR.status + ' ' + errorThrown);
                            }
                        });
                    },
                    error: function(jqXHR, textStatus, errorThrown)
                    {
                        f3.showmessage('Error getting GameXML: ' + jqXHR.status + ' ' + errorThrown);
                    }
                });
            }
            else
            {
                // get the cached LiveInfo
                actionurl = 'getLiveInfo';

                $.ajax(
                {
                    type: 'get',
                    cache: false,
                    dataType: 'json',
                    url: actionurl,
                    success: function(json, textStatus, jqXHR)
                    {
                        // if cached, parse the data
                        f3.data.liveinfo = json;
                        f3.data.parse(f3.data, group);
                    },
                    error: function(jqXHR, textStatus, errorThrown)
                    {
                        if (jqXHR.status === 404)
                        {
                            // if not found, get the xml again instead
                            f3.groups.liveinfo('liveinfo', true);
                        }
                        else
                        {
                            f3.showmessage('Error getting LiveInfo: ' + jqXHR.status + ' ' + errorThrown);
                        }
                    }
                });
            }
        },

        profileinfo: function(group, refresh)
        {
            var actionurl = 'getProfileInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.profileinfo = json;
                    f3.data.parse(f3.data, group);

                    //    f3.groups.achievementinfo('achievementinfo');
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting ProfileInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        achievementinfo: function(group, refresh)
        {
            var process = function()
            {
                var actionurl = 'getPlayerAchievements';

                $.ajax(
                {
                    type: 'get',
                    cache: false,
                    dataType: 'json',
                    url: actionurl,
                    success: function(json, textStatus, jqXHR)
                    {
                        for (var i = 0; i < f3.data.achievementinfo.length; i++)
                        {
                            $.each(json, function(index, playerachievement)
                            {
                                if (f3.data.achievementinfo[i].id == playerachievement.id)
                                {
                                    f3.data.achievementinfo[i].player = playerachievement.player;
                                    return false;
                                }
                            });
                        }
                    },
                    error: function(jqXHR, textStatus, errorThrown)
                    {
                        f3.showmessage('Error getting PlayerAchievements: ' + jqXHR.status + ' ' + errorThrown);
                    },
                    complete: function()
                    {
                        f3.data.parse(f3.data, group);
                    }
                });
            }


            if (refresh)
            {
                //check signed in profiles again
                f3.groups.profileinfo('profileinfo', true);
                process();
            }
            else
            {
                var actionurl = 'getAchievementInfo';

                $.ajax(
                {
                    type: 'get',
                    cache: false,
                    dataType: 'json',
                    url: actionurl,
                    success: function(json, textStatus, jqXHR)
                    {
                        f3.data.achievementinfo = json;
                        process();
                    },
                    error: function(jqXHR, textStatus, errorThrown)
                    {
                        f3.showmessage('Error getting AchievementInfo: ' + jqXHR.status + ' ' + errorThrown);
                    }
                });
            }
        },

        gameinfo: function(group, refresh)
        {
            var actionurl = 'getGameInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.gameinfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting GameInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        systeminfo: function(group, refresh)
        {
            var actionurl = 'getSystemInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.systeminfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting SystemInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        plugininfo: function(group, refresh)
        {
            var actionurl = 'getPluginInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.plugininfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting PluginInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        memoryinfo: function(group, refresh)
        {
            var actionurl = 'getMemoryInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.memoryinfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting MemoryInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        threadinfo: function(group, refresh)
        {
            var actionurl = 'getThreadInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.threadinfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting ThreadInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        systemlinkinfo: function(group, refresh)
        {
            var actionurl = 'getSystemLinkInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.systemlinkinfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting SystemLinkInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        temperatureinfo: function(group, refresh)
        {
            var actionurl = 'getTemperatureInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.temperatureinfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting TemperatureInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        systemlinkbandwidth: function(group, refresh)
        {
            var actionurl = 'getSystemLinkBandwidth';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.systemlinkbandwidth = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting SystemLinkBandwidth: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        multidiscinfo: function(group, refresh)
        {
            var actionurl = 'getMultiDiscInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.multidiscinfo = json;
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting MultiDiscInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        screencapture: function(group, refresh)
        {
            if (refresh === true)
            {
                //get an actual screencapture

                var actionurl = 'getScreenCapture';

                $.ajax(
                {
                    type: 'get',
                    cache: false,
                    dataType: 'json',
                    url: actionurl,
                    success: function(json, textStatus, jqXHR)
                    {
                        f3.data.screencapture = json;
                        f3.data.parse(f3.data, group);

                        if (typeof f3.data.screencaptures === 'undefined') f3.data.screencaptures = [];
                        f3.data.screencaptures.unshift(f3.data.screencapture);
                        f3.data.parse(f3.data, 'screencaptures');
                    },
                    error: function(jqXHR, textStatus, errorThrown)
                    {
                        f3.showmessage('Error getting ScreenCapture: ' + jqXHR.status + ' ' + errorThrown);
                    }
                });
            }
            else
            {
                //parse the data if exist
                if (typeof f3.data.screencapture !== 'undefined') f3.data.parse(f3.data, group);
            }
        },

        screencaptures: function(group, refresh)
        {      
            var actionurl = 'getScreenCaptures';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    f3.data.screencaptures = json.reverse();
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting ScreenCaptures: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        dashlaunchinfo: function(group, refresh)
        {
            var actionurl = 'getDashLaunchInfo';

            $.ajax(
            {
                type: 'get',
                cache: false,
                dataType: 'json',
                url: actionurl,
                success: function(json, textStatus, jqXHR)
                {
                    var dashlaunchinfo = { version: json.version, categories: {} };
                    $.each(json.options, function(index, option)
                    {
                        var category = option.category.toLowerCase();
                        if (typeof dashlaunchinfo.categories[category] == 'undefined') dashlaunchinfo.categories[category] = {};
                        dashlaunchinfo.categories[category][option.name] = option.value;
                    });

                    $.extend(f3.data.dashlaunchinfo, dashlaunchinfo);
                    f3.data.parse(f3.data, group);
                },
                error: function(jqXHR, textStatus, errorThrown)
                {
                    f3.showmessage('Error getting DashLaunchInfo: ' + jqXHR.status + ' ' + errorThrown);
                }
            });
        },

        userdata: function(group, refresh)
        {
            if (refresh === false || typeof f3.data.userdata === 'undefined')
            {
                f3.data.userdata = new f3.classes.userdata();
            }

            f3.data.userdata.load(
            {
                verbose: false,
                error: function(userdata, jqXHR, textStatus, errorThrown)
                {
                    if (jqXHR.status == '404')
                    {
                        userdata.save(
                        {
                            verbose: true,
                            complete: function()
                            {
                                f3.data.parse(f3.data, group);
                            }
                        });
                    }
                    else
                    {
                        f3.showmessage('Error loading UserData: ' + jqXHR.status + ' ' + errorThrown);
                    }
                },
                complete: function()
                {
                    f3.data.parse(f3.data, group);
                    f3.groups.screencaptures('screencaptures', true);
                }
            });
        }
    },

    showmessage: function(message)
    {
        if ($('#message span').html().toUpperCase().search(message.toUpperCase()) != -1) return;
        $($('#message span').append(message).append('<br />').parent()).fadeIn(500);
    },

    run: function()
    {
        //scroll to active (in case of refresh)
        //scrollOnLoad();
        selectOnScroll();
        //$(window).on('scroll', selectOnScroll);

        //turn of cache
        $.ajaxSetup(
        {
            cache: false
        });

        //bind all functions
        f3.bindall();

        //initialize the graphs
        f3.behavior.graphs.initialize();

        //execute all
        $.each(f3.groups, function(n, v)
        {
            v(n);
        });

        setInterval(function()
        {
            //refresh groups if needed
            $.each(f3.settings.timer.groups, function(n, v)
            {
                f3.groups[v](v, true);
            });
        }, f3.settings.timer.interval);
    }
}
var folder = (typeof f3.parameters.folder !== 'undefined') ? (f3.parameters.folder + '/') : '_mw3new/';

$(document).ready(function()
{
    f3.run();
});