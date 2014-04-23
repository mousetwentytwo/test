/// <reference path="f3.plugin.common.js" />

f3.extend('behavior').tools =
{
    'bytestoreadable': function(bytes)
    {
        var s = ['B', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB'];
        var e = Math.floor(Math.log(bytes) / Math.log(1024)) | 0;
        return (bytes / Math.pow(1024, Math.floor(e))).toFixed(2) + ' ' + s[e];
    },
    
    'truncate': function(string, length, offcenter)
    {
        if (typeof offcenter === 'undefined') offcenter = 0;
        if (typeof length === 'undefined' || string.length < length) return string;
        var split = Math.floor((length - 4) / 2);
        var s1 = string.substr(0, split + offcenter);
        var s2 = string.substring(string.length - split + offcenter);
        return s1 + '...' + s2;
    },
    
    'pathtoreadable': function(path)
    {
        $.each(f3.common.defines.drivemounts, function(n, v)
        {
            if (path.slice(0, n.length).toUpperCase() == n.toUpperCase())
            {
                path = path.replace(path.slice(0, n.length), v);
                return false;
            }
        });
        return path;
    },

    'getfirstsignedingamertag': function()
    {
        var gamertag = '';
        if (typeof f3.data.profileinfo == 'undefined') return gamertag;
        $.each(f3.data.profileinfo, function(index, profile)
        {
            if (profile.signedin == 1)
            {
                gamertag = profile.gamertag;
                return false;
            }                            
        });
        return gamertag;
    },

    'getsignedinprofilescount': function()
    {
        var profiles = 0;
        if (typeof f3.data.profileinfo !== 'undefined')
        {
            for (var i = 0; i < f3.data.profileinfo.length; i++) profiles += f3.data.profileinfo[i].signedin;
        }
        return profiles;
    },

    'getachievementunlockcount': function(achievement)
    {
        var unlocked = 0;
        for (var i = 0; i < achievement.player.length; i++) unlocked += achievement.player[i];
        return unlocked;                
    },

    'getachievementshowsecret': function()
    {
        if (typeof f3.data.userdata == 'undefined' ||
            typeof f3.data.userdata.settings == 'undefined' ||
            typeof f3.data.userdata.settings.achievements == 'undefined' ||
            typeof f3.data.userdata.settings.achievements.showsecret == 'undefined') return false;
        return f3.data.userdata.settings.achievements.showsecret;
    }
}



f3.extend('behavior.graphs').initialize = function()
{
    var temperatureinfo = f3.extend('behavior.graphs.temperatureinfo');

    temperatureinfo.filled = false;
    temperatureinfo.maxdata = 100;
    temperatureinfo.celsius = true;

    temperatureinfo.graph = new Rickshaw.Graph(
    {
        element: $('.graph.temperature .chart').get(0),
        //width: 500,
        //height: 250,
        min: 15, //59F
        max: 110, //230F
        renderer: 'line',
        stroke: true,
        series: new Rickshaw.Series.FixedDuration(
        [
		    {
		        name: 'Case',
		        color: '#6699ff'
		    },
            {
                name: 'Memory',
                color: '#9dfd9d'
            },
            {
                name: 'GPU',
                color: '#fdfd9d'
            },
            {
                name: 'CPU',
                color: '#ee6b6b'
            }
	    ],
        undefined,
        {
            timeInterval: f3.settings.timer.interval,
            maxDataPoints: temperatureinfo.maxdata,
            timeBase: 0 - (f3.settings.timer.interval * temperatureinfo.maxdata / 1000) //new Date().getTime() / 1000
        })
    });

    temperatureinfo.hoverdetail = new Rickshaw.Graph.HoverDetail(
    {
        graph: temperatureinfo.graph,
        xFormatter: function(x)
        {
            var minutes = Math.floor(x / 60);
            var seconds = x - minutes * 60;
            return minutes + ':' + (seconds < 10 ? '0' : '') + seconds;
        },
        yFormatter: function(y)
        {
            return y + '&deg;' + (temperatureinfo.celsius == true ? 'C' : 'F');
        }
    });


    temperatureinfo.legend = new Rickshaw.Graph.Legend(
    {
        graph: temperatureinfo.graph,
        element: $('.graph.temperature .legend').get(0)
    });

    temperatureinfo.highlighter = new Rickshaw.Graph.Behavior.Series.Highlight(
    {
        graph: temperatureinfo.graph,
        legend: temperatureinfo.legend
    });

    temperatureinfo.xAxis = new Rickshaw.Graph.Axis.Time(
    {
        graph: temperatureinfo.graph,
        ticksTreatment: 'glow',
        element: $('.graph.temperature .timeline').get(0)
    });

    temperatureinfo.xAxis.render();

    temperatureinfo.yAxis = new Rickshaw.Graph.Axis.Y(
    {
        graph: temperatureinfo.graph,
        tickFormat: Rickshaw.Fixtures.Number.formatKMBT,
        ticksTreatment: 'glow'
    });

    temperatureinfo.yAxis.render();

    temperatureinfo.switchunit = function(celsius)
    {
        temperatureinfo.celsius = (typeof celsius === 'undefined') ? true : celsius;
        if (temperatureinfo.celsius == true)
        {
            temperatureinfo.graph.min = 15;
            temperatureinfo.graph.max = 110;
        }
        else
        {
            temperatureinfo.graph.min = 60; //59 == 15
            temperatureinfo.graph.max = 230;
        }
    };

    var memoryinfo = f3.extend('behavior.graphs.memoryinfo');

    memoryinfo.filled = false;
    memoryinfo.maxdata = 100;

    memoryinfo.graph = new Rickshaw.Graph(
    {
        element: $('.graph.memory .chart').get(0),
        //width: 500,
        //height: 250,
        min: 0,
        //max: 512,
        renderer: 'area',
        stroke: true,
        series: new Rickshaw.Series.FixedDuration(
        [
		    {
		        name: 'Used',
		        color: '#ee6b6b'
		    },
            {
                name: 'Free',
                color: '#9dfd9d'
            }
        ],
        undefined,
        {
            timeInterval: f3.settings.timer.interval,
            maxDataPoints: memoryinfo.maxdata,
            timeBase: 0 - (f3.settings.timer.interval * memoryinfo.maxdata / 1000) //new Date().getTime() / 1000
        })
    });


    memoryinfo.hoverdetail = new Rickshaw.Graph.HoverDetail(
    {
        graph: memoryinfo.graph,
        xFormatter: function(x)
        {
            var minutes = Math.floor(x / 60);
            var seconds = x - minutes * 60;
            return minutes + ':' + (seconds < 10 ? '0' : '') + seconds;
        },
        yFormatter: function(y)
        {
            return f3.behavior.tools.bytestoreadable(y);
        }
    });

    memoryinfo.legend = new Rickshaw.Graph.Legend(
    {
        graph: memoryinfo.graph,
        element: $('.graph.memory .legend').get(0)
    });

    memoryinfo.highlighter = new Rickshaw.Graph.Behavior.Series.Highlight(
    {
        graph: memoryinfo.graph,
        legend: memoryinfo.legend
    });

    memoryinfo.xAxis = new Rickshaw.Graph.Axis.Time(
    {
        graph: memoryinfo.graph,
        ticksTreatment: 'glow',
        element: $('.graph.memory .timeline').get(0)
    });

    memoryinfo.xAxis.render();

    memoryinfo.yAxis = new Rickshaw.Graph.Axis.Y(
    {
        graph: memoryinfo.graph,
        tickFormat: Rickshaw.Fixtures.Number.formatKMBT,
        ticksTreatment: 'glow'
    });

    memoryinfo.yAxis.render();

    

    var systemlinkbandwidth = f3.extend('behavior.graphs.systemlinkbandwidth');

    systemlinkbandwidth.filled = false;
    systemlinkbandwidth.maxdata = 100;

    systemlinkbandwidth.maxupstream = 0;
    systemlinkbandwidth.maxdownstream = 0;

    systemlinkbandwidth.graph = new Rickshaw.Graph(
    {
        element: $('.graph.bandwidth .chart').get(0),
        //width: 500,
        //height: 250,
        min: 0,
       // max: 350,
        renderer: 'line',
        stroke: true,
        series: new Rickshaw.Series.FixedDuration(
        [
		    {
		        name: 'Downstream',
		        color: '#6699ff'
		    },
            {
                name: 'Upstream',
                color: '#fdfd9d'
            }
	    ],
        undefined,
        {
            timeInterval: f3.settings.timer.interval,
            maxDataPoints: systemlinkbandwidth.maxdata,
            timeBase: 0 - (f3.settings.timer.interval * systemlinkbandwidth.maxdata / 1000) //new Date().getTime() / 1000
        })
    });

    systemlinkbandwidth.hoverdetail = new Rickshaw.Graph.HoverDetail(
    {
        graph: systemlinkbandwidth.graph,
        xFormatter: function(x)
        {
            var minutes = Math.floor(x / 60);
            var seconds = x - minutes * 60;
            return minutes + ':' + (seconds < 10 ? '0' : '') + seconds;
        },
        yFormatter: function(y)
        {
            return f3.behavior.tools.bytestoreadable(y);
        }
    });


    systemlinkbandwidth.legend = new Rickshaw.Graph.Legend(
    {
        graph: systemlinkbandwidth.graph,
        element: $('.graph.bandwidth .legend').get(0)
    });

    systemlinkbandwidth.highlighter = new Rickshaw.Graph.Behavior.Series.Highlight(
    {
        graph: systemlinkbandwidth.graph,
        legend: systemlinkbandwidth.legend
    });

    systemlinkbandwidth.xAxis = new Rickshaw.Graph.Axis.Time(
    {
        graph: systemlinkbandwidth.graph,
        ticksTreatment: 'glow',
        element: $('.graph.bandwidth .timeline').get(0)
    });

    systemlinkbandwidth.xAxis.render();

    systemlinkbandwidth.yAxis = new Rickshaw.Graph.Axis.Y(
    {
        graph: systemlinkbandwidth.graph,
        tickFormat: Rickshaw.Fixtures.Number.formatKMBT,
        ticksTreatment: 'glow'
    });

    systemlinkbandwidth.yAxis.render();

}

f3.extend('behavior').events =
{
    'liveinfo.images.background':
    {
        'background':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (f3.parameters.test == 1) 
                {
                    //for local testing
                    return value.replace(/[\?=]/gi,'/');
                }
                return value;
            }
        }
    },
    
    'liveinfo.images.icon':
    {
        'background':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (f3.parameters.test == 1) 
                {
                    //for local testing
                    return value.replace(/[\?=]/gi,'/');
                }
                return value;
            },

            'onLoadSucceeded': function(object, value)
            {
                $(object).animate({ marginLeft: 0 }, 200, function()
                {
                    $(this).fadeTo(200, 1);
                    return value;
                });
            },

            'onLoadFailed': function(object, value)
            {
                f3.showmessage('Failed to load images');
                return value;
            }
        }
    },

    'liveinfo.images.boxart.large':
    {
        'background':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (f3.parameters.test == 1) 
                {
                    //for local testing
                    return value.replace(/[\?=]/gi,'/');
                }
                return value;
            },

            'onLoadSucceeded': function(object, value)
            {
                $(object).slideDown(400, function()
                {
                    $(this).fadeTo(200, 1);
                    return value;
                });
            },

            'onLoadFailed': function(object, value)
            {
                f3.showmessage('Failed to load images');
                return value;
            }
        }
    },

    'liveinfo.ratingaggregate':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                value -= $(object).data('part') !== 'undefined' ? $(object).data('part') - 1 : 5;
                if (value >= 1) value = 1;
                $(object).addClass('f3 star' + (value * 100));
                return ''; //false; // cancels the actual set value
            },
            'onAfterSetValue': function(object, value)
            {
                return value;
            }
        }
    },

    'liveinfo.numberofratings':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return ' / ' + value;
            }
        }
    },

    'liveinfo.categories':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                var s = '';
                $.each(value, function(n, v)
                {
                    if ($.inArray(v.categoryid, f3.common.defines.validgamegenres) >= 0)
                    {
                        s += (s.length > 0 ? ', ' : '') + v.name;
                    }
                });
                return s;
            }
        }
    },

    'liveinfo.description':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                return String(value).replace(/\n/gi, '<br />');
            }
        }
    },

    'liveinfo.gamecapabilities.offlinedolbydigital':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (value > 0)
                {
                    value = 'Dolby Digital';
                    $(object).show();
                }
                else
                {
                    $(object).hide();
                }
                return value;
            }
        }
    },

    'liveinfo.gamecapabilities.onlinecontentdownload':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (value > 0)
                {
                    value = 'Content download';
                    $(object).show();
                }
                else
                {
                    $(object).hide();
                }
                return value;
            }
        }
    },

    'liveinfo.gamecapabilities.onlineleaderboards':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (value > 0)
                {
                    value = 'Leaderboards';
                    $(object).show();
                }
                else
                {
                    $(object).hide();
                }
                return value;
            }
        }
    },

    'liveinfo.gamecapabilities.onlinevoice':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (value > 0)
                {
                    value = 'Voice chat';
                    $(object).show();
                }
                else
                {
                    $(object).hide();
                }
                return value;
            }
        }
    },

    'liveinfo.images.screenshots':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var html = "";
                $.each(value, function(n, v)
                {
                    if (f3.parameters.test == 1)
                    {
                        //for local testing
                        v = v.replace(/[\?=]/gi,'/');
                    }
                    html += '<div class="f3 screenshot" data-src="' + v + '"></div>';
                });

                return html;
            },

            'onAfterSetValue': function(object, value)
            {
                $('#slides').camera(
                {
                    pagination: true,
                    thumbnails: false,
                    navigation: true,
                    loader: 'none',
                    fx: 'scrollHorz',
                    time: 4000,
                    portrait: true,
                    height: '56%', //assuming 16:9 aspect ratio
                    onLoaded: function()
                    {
                        //$($(object).parent()).fadeIn(1000);
                        $($(object).parent()).fadeTo(1000, 1);
                    }
                });
            }
        }
    },

    'gameinfo.path':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                value = f3.behavior.tools.pathtoreadable(value);
                $(object).attr('title', value);
                return f3.behavior.tools.truncate(value, 34, -5);

            }
        }
    },

    'plugininfo.version.number.type':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return (typeof f3.common.defines.versiontypes[value] !== 'undefined' ? f3.common.defines.versiontypes[value] : value);
            }
        }
    },

    'plugininfo.path.root':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.pathtoreadable(value);
            }
        }
    },

    'plugininfo.path.user':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.pathtoreadable(value);
            }
        }
    },

    'plugininfo.path.web':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.pathtoreadable(value);
            }
        }
    },

    'plugininfo.path.launcher':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                if ($(object).is('div'))
                {
                    return $(object).click(function(event)
                    {
                        if (confirm('Are you sure you want to return to launcher?\nThis will end the current game or application...') == false) return false;
                        var folders = value.split('\\');
                        var executable = folders.pop();
                        var path = folders.join('\\');
                        var extension = executable.substring(executable.lastIndexOf('.') + 1);
                        var exectype = (extension == executable) ? f3.common.defines.launchdata.exectype.xexcon : f3.common.defines.launchdata.exectype[extension];
                        
                        var actionurl = 'postLaunchGame';
                
                        $.ajax(
                        {
                            type: 'post',
                            cache: false,
                            dataType: 'text',
                            url: actionurl,
                            data: { "path": path, "exec": executable, "type": exectype },
                            success: function(text, textStatus, jqXHR)
                            {
                                setTimeout("window.location.reload(true)", 500)
                            },
                            error: function(jqXHR, textStatus, errorThrown)
                            {
                            
                            },
                            complete: function(jqXHR, textStatus)
                            {
                                return value;
                            }
                        });
                    }).text();
                }
                return f3.behavior.tools.pathtoreadable(value);
            }
        }
    },

    'plugininfo.features':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var html = '';
                var part = $(object).data('part');
                $.each(value, function(n, v)
                {
                    if ((part === 'yes' && v === 1) || (part === 'no' && v === 0))
                    {
                        var display = typeof f3.common.defines.features[n] === 'undefined' ? n[0].toUpperCase() + n.slice(1) : f3.common.defines.features[n];
                        html += '<li class="f3 ' + part + '">' + display + '</li>';
                    }
                });
                return html;
            }
        }
    },

    'memoryinfo.used':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.bytestoreadable(value);
            }
        }
    },

    'memoryinfo.free':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.bytestoreadable(value);
            }
        }
    },

    'memoryinfo.total':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.bytestoreadable(value);
            }
        }
    },

    'systeminfo.cpukey':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (value === '')
                {
                    value = 'not available';
                    $(object).addClass('f3 notapplicable');
                }
                return value;
            }
        }
    },

    'systeminfo.dvdkey':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (value === '')
                {
                    value = 'not available';
                    $(object).addClass('f3 notapplicable');
                }
                return value;
            }
        }
    },

    'systemlinkinfo.enabled':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return value == 1 ? 'Yes' : 'No';
            }
        }
    },

    'multidiscinfo.entries':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var disc = $(object).data('part') !== 'undefined' ? $(object).data('part') - 1 : -1;
                if (disc < 0) return value;
                var total = $('[data-name="multidiscinfo.disc.total"]').text();
                var current = $('[data-name="multidiscinfo.disc.current"]').text();
                if (disc >= total)
                {
                    $(object).hide();
                    return value;
                }
                var html = '';
                var path = value[disc].path;
                if (path !== '')
                {
                    html = (value[disc].container == 1) ? '[container] ' : '[extracted] ';
                    html += f3.behavior.tools.pathtoreadable(path);
                    if (disc + 1 == current) $(object).addClass('f3 active');
                }
                else
                {
                    html = 'missing';
                    $(object).addClass('f3 notapplicable');
                }
                return html;
            }
        }
    },

    'dashlaunchinfo.categories':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                var html = $('<div></div>');
                var group = 'dashlaunchinfo';
                $.each(value, function(category, options)
                {
                    var context = $($(object).data('html'));
                    var data =
                    {
                        dashlaunchinfo:
                        {
                            category: category,
                            options: options
                        }
                    };
                    f3.data.parse(data, group, context);
                    $(html).append(context);
                });
                return $(html).html();
            }
        }
    },

    'dashlaunchinfo.category':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                var translations = f3.data.dashlaunchinfo.translations;
                return ((typeof translations['opt_fil_' + value] == 'undefined') ? translations['oopsie'].webui : translations['opt_fil_' + value].webui);
            }
        }
    },

    'dashlaunchinfo.options':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                var html = $('<div></div>');
                var group = 'dashlaunchinfo';
                $.each(value, function(option, value)
                {
                    var context = $($(object).data('html'));
                    var data =
                    {
                        dashlaunchinfo:
                        {
                            option:
                            {
                                name: option,
                                description: option,
                                value: value
                            }
                        }
                    };
                    f3.data.parse(data, group, context);
                    $(html).append(context);
                });
                return $(html).html();
            }
        }
    },

    'dashlaunchinfo.option.description':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var translations = f3.data.dashlaunchinfo.translations;
                return ((typeof translations[value] == 'undefined') ? translations['oopsie'].webui : translations[value].webui);
            }
        }
    },

    'dashlaunchinfo.option.value':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                if ($(object).siblings().filter('[data-name="dashlaunchinfo.option.name"]').text() == 'region')
                {
                    value = (typeof f3.data.dashlaunchinfo.translations.regions[value] != 'undefined') ? f3.data.dashlaunchinfo.translations.regions[value] + ' (' + value + ')' : 'Custom/Unknown (' + value + ')';
                }
                return (value == '') ? '<span class="f3 notapplicable">not set</span>' : value;
            }
        }
    },

    'dashlaunchinfo.version.number.minor':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return ('0' + value).slice(-2);
            }
        }
    },

    'threadinfo':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var time = 50; //animation speed base

                var addrow = function(threads, pos)
                {
                    if (typeof pos !== 'number') pos = 0;
                    var rows = threads.length;
                    var exist = $(object).find('tr:has("td")').length;

                    var html = '';
                    if (rows > 0)
                    {
                        $.each(threads[0], function(n, v)
                        {
                            html += '<td class="f3 ' + n + '">' + v + '</td>';
                        });
                    }

                    if (pos < exist && rows > 0)
                    {
                        $(object).find('tr:has("td"):nth(' + pos + ')').fadeTo(time, 0, function()
                        {
                            $(this).html(html);
                            $(this).fadeTo(time * 2, 1);
                            addrow(threads.splice(1), ++pos);
                        });
                        return;
                    }

                    if (rows > 0)
                    {
                        var row = $(document.createElement('tr'));
                        row.html(html);
                        row.css('opacity', 0);
                        $(object).append(row);
                        setTimeout(function()
                        {
                            row.fadeTo(time * 2, 1);
                            addrow(threads.slice(1), ++pos);
                        }, time);
                        return;
                    }

                    if (pos < exist)
                    {
                        $(object).find('tr:has("td"):nth(' + pos + ')').fadeTo(time, 0, function()
                        {
                            $(this).remove();
                            addrow(threads, pos);
                        });

                    }
                }

                if (value.length > 0 && $(object).find('tr:has("th")').length == 0)
                {
                    setTimeout(function()
                    {
                        var tr = $(document.createElement('tr'));
                        tr.css('opacity', 0);
                        var html = '';
                        $.each(value[0], function(n, v)
                        {
                            html += '<th class="f3 ' + n + '">' + n + '</th>';
                        });
                        tr.html(html);
                        $(object).append(tr);
                        tr.fadeTo(time * 2, 1, function()
                        {
                            addrow(value);
                        });
                    }, 200); // TODO: use 'time * ?'
                }
                else
                {
                    addrow(value);
                }

                return $(object).html();
            }
        },
        
        'text':
        {
            'onBeforeRefreshOnce': function(object, value)
            {
                var actionurl = 'postThreadStateChange';
                
                var suspend = $(object).data('suspend') || 0;

                $.ajax(
                {
                    type: 'post',
                    cache: false,
                    dataType: 'text',
                    url: actionurl,
                    data: { "suspend": suspend },
                    success: function(text, textStatus, jqXHR)
                    {
                        // jqXHR.status 200 -> state changed to requested state
                        // jqXHR.status 304 -> state was already requested state
                    },
                    error: function(jqXHR, textStatus, errorThrown)
                    {
                        // jqXHR.status 500 -> state failed to change to requested state
                    },
                    complete: function(jqXHR, textStatus)
                    {
                        return value;
                    }
                });

                return 500; //delay
            }
        }
    },

    'screencapture':
    {
        'text':
        {
            'onBeforeRefreshOnce': function(object, value)
            {
                $(object).parent().parent().find('.container').fadeIn(400).find('.loader').fadeIn(400);
                return 0;
            }
        }
    },

    'screencapture.filename':
    {
        'image':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                if (f3.parameters.test == 1)
                {
                    //for local testing
                    return 'getScreenCaptureImage%20uuid=' + value;
                }
                return 'getScreenCaptureImage?uuid=' + value;
            },

            'onLoadSucceeded': function(object, value, data, image)
            {
                $(image).css('maxWidth', image.width).addClass('f3 screencapture image');
                $(object).parent().find('.loader').fadeOut(400, function()
                {
                    $(object).parent().slideDown(400, function()
                    {
                        $(object).fadeTo(400, 1);
                        return value;
                    });
                });
            },

            'onLoadFailed': function(object, value, data, image)
            {
                f3.showmessage('Failed to load Screen Capture');
                $(object).parent().find('.loader').fadeOut(400);
                return value;
            }
        }
    },

    'screencapture.timestamp':
    {
        'date':
        {
            'onBeforeFormatValue': function(object, value)
            {
                var delimiters = ['-', '-', ' ', ':', ':', '.', ''];
                var newvalue = '';
                var r = /(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})(\d{1,3})/;
                $.each(r.exec(value).slice(1), function(i, part)
                {
                    newvalue += part + delimiters[i];
                });
                return newvalue;
            }
        }
    },

    'screencaptures':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var html = $('<div></div>');
                $.each(value, function(index, metadata)
                {
                    var tmp = $($(object).data('html'));
                    var data = { 'screencaptures': metadata};
                    f3.data.parse(data, 'screencaptures', tmp);
                    tmp.attr('id', metadata.filename);
                    $(html).append(tmp);               
                });
                return $(html).html();
            },

            'onAfterSetValue': function(object, value, data, image)
            {
                var url = 'getScreenCaptureImage?uuid=';
                if (f3.parameters.test == 1)
                {
                    //for local testing
                    url = 'getScreenCaptureImage%20uuid=';
                }
                $(object).find('tr').unbind('click').click(function()
                {
                    $.fancybox.open(url + this.id,
                    {
                        autoResize: false,
                        arrows: false,
                        type: 'image',
                        afterShow: function()
                        {
                            $('.fancybox-image').jQueryNotes(
                            {
                                operator: f3.data.userdata,
                                author: f3.behavior.tools.getfirstsignedingamertag(),
                                addImmediately: true
                            });
                        }
                    });
                }).each(function(a, b, c)
                {
                    var notes = f3.data.userdata.notes[url + this.id];
                    notes = typeof notes === 'undefined' ? '' : notes.length;
                    
                    $(this).find('span.notecount').text(notes);               
                });

                $(object).find('input').unbind('click').click(function(event)
                {
                    event.stopPropagation();
                });

                $(object).siblings('tbody').find('input#screencapturesall').checkAll('tbody[data-name="screencaptures"] input:checkbox');
                
                $('#deletescreencaptures').unbind('click').click(function(event)
                {
                    var checked = $(this).parents('table').find('input:checked').not('input#screencapturesall').toArray();
                    if (checked.length == 0) return;
                    if (window.confirm('Delete the selected screencaptures?\nThis includes any notes attached to these screencaptures.') == false) return;
                    
                    
                    function deletescreencapture(screencaptures)
                    {
                        console.log(screencaptures);
                        //abort when all screencaptures are deleted
                        if (screencaptures.length == 0) return value;

                        var actionurl = 'postScreenCaptureDelete';
                        var uuid = $(screencaptures.shift()).data('uuid');
                        
                        console.log(screencaptures, uuid);

                        $.ajax(
                        {
                            type: 'post',
                            cache: false,
                            dataType: 'text',
                            url: actionurl,
                            data: { "uuid": uuid },
                            success: function(text, textStatus, jqXHR)
                            {
                                var url = 'getScreenCaptureImage?uuid=';
                                if (f3.parameters.test == 1)
                                {
                                    //for local testing
                                    url = 'getScreenCaptureImage%20uuid=';
                                }
                                
                                //remove notes for deleted screencapture
                                if (typeof f3.data.userdata.notes[url + uuid] !== 'undefined')
                                {
                                    delete f3.data.userdata.notes[url + uuid];
                                    f3.data.userdata.save();
                                }

                                //remove deleted screencapture from data
                                for (var i = 0; i < f3.data.screencaptures.length; i++)
                                {
                                    if (f3.data.screencaptures[i].filename == uuid)
                                    {
                                        f3.data.screencaptures.splice(i, 1);
                                        break;
                                    }
                                }

                                //remove deleted screencapture from table
                                $(object).find('tr#' + uuid).hide(200).remove();
                            },
                            error: function(jqXHR, textStatus, errorThrown)
                            {
                                //failed.push(uuid);

                                //color failed deletes red
                                $(object).find('tr#' + uuid).css('backgroundColor', '#9e1c1c');
                            },
                            complete: function(jqXHR, textStatus)
                            {
                                //delete the next screencapture
                                deletescreencapture(screencaptures);
                            }
                        });
                    }

                    //delete screencaptures recursively one by one
                    deletescreencapture(checked);
                });
            }
        }
    },

    'screencaptures.timestamp':
    {
        'date':
        {
            'onBeforeFormatValue': function(object, value)
            {
                var delimiters = ['-', '-', ' ', ':', ':', '.', ''];
                var newvalue = '';
                var r = /(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})(\d{1,3})/;
                $.each(r.exec(value).slice(1), function(i, part)
                {
                    newvalue += part + delimiters[i];
                });
                return newvalue;
            }
        }
    },

    'screencaptures.filesize':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.bytestoreadable(value);
            }
        }
    },

    'profileinfo':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var index = (typeof $(object).data('index') !== 'undefined') ? $(object).data('index') : 0;
                var html = $($(object).data('html'));
                var profileinfo = f3.data.profileinfo[index];
                f3.data.parse({ profileinfo: profileinfo }, 'profileinfo', html);

                //only add background to div
                if ($(object).is('div')) $(object).css('background-image', 'url(\'images/profile.' + index + '.' + (profileinfo.signedin == 1 ? 'in' : 'out') + '.png\')');
                return html;
            },

            'onAfterSetValue': function(object, value, data)
            {
                if ($(object).is('th')) $(object).removeClass('hide').width(value.width() + 12).addClass('hide');
            }
        }
    },

    'profileinfo.gamertag':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                return (value == '' ? 'Not signed in' : value);
            }
        }
    },

    'profileinfo.index':
    {
        'background':
        {
            'onBeforeSetValue': function(object, value)
            {
                if (f3.parameters.test == 1)
                {
                    //for local testing
                    return 'getProfileImage%20uuid=' + value;
                }
                return 'getProfileImage?uuid=' + value;
            },

            'onLoadSucceeded': function(object, value)
            {
                var index = parseInt(value.slice(-1));
                if (isNaN(index)) return value;

                if (index % 2 == 0)
                {
                    $(object).animate({ marginLeft: 0 }, 200, function()
                    {
                        $(this).fadeTo(200, 1);
                        return value;
                    });
                }
                else
                {
                    $(object).animate({ marginRight: 0 }, 200, function()
                    {
                        $(this).fadeTo(200, 1);
                        return value;
                    });
                }
            }
        }
    },

    'achievementinfo':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var html = $('<div></div>');
                $.each(value, function(index, achievement)
                {
                    var tmp = $($(object).data('html'));
                    var data = { 'achievementinfo': achievement};
                    f3.data.parse(data, 'achievementinfo', tmp);
                    //tmp.attr('id', metadata.filename);
                    $(html).append(tmp);               
                });
                return $(html).html();
            },

            'onAfterSetValue': function(object, value)
            {
                if (typeof f3.data.profileinfo !== 'undefined')
                {
                    for (var i = 0; i < 4; i++)
                    {
                        if (f3.data.profileinfo[i].signedin == 1)
                        {
                            $(object).parent().find('tr > :nth-last-child(' + (4 - i) + ')').removeClass('hide');
                        }
                        else
                        {
                            $(object).parent().find('tr > :nth-last-child(' + (4 - i) + ')').addClass('hide');
                        }
                    }
                }

                return value;
            }
        }
    },

    'achievementinfo.imageid':
    {
        'image':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                $(object).attr('data-imageid', value);

                if (data.achievementinfo.hidden == 1 && f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0 && f3.behavior.tools.getachievementshowsecret() == false)
                {
                    return 'images/secretachievement.png';
                }

                if (f3.parameters.test == 1)
                {
                    //for local testing
                    return 'getAchievementImage%20uuid=' + value;
                }
                return 'getAchievementImage?uuid=' + value;
            },

            'onLoadSucceeded': function(object, value, data)
            {
                var fadeimage = f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0 ? 0.4 : 1;
                var fadesecret = (data.achievementinfo.hidden == 0 || f3.behavior.tools.getachievementunlockcount(data.achievementinfo) > 0) ? 0 : 0.4;
                //reference is lost in context parsing, must reset the src
                $('[data-imageid=' + $(object).data('imageid') + ']').attr('src', value)
                    .animate({ marginLeft: 0 }, 200, function()
                    {
                        $(this).fadeTo(200, fadeimage).css('borderColor', data.achievementinfo.hidden == 1 ? '#9e1c1c' : 'black')
                            .siblings('.f3.secret').fadeTo(200, fadesecret);//.css('borderColor', data.achievementinfo.hidden == 1 ? 'red' : 'black');
                        return value;
                    });
            }
        }
    },

    'achievementinfo.strings.caption':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                // if it's a secret achievement and no profile unlocked the achievement and the checkbox for showing secret achievements is not set, hide it
                if (data.achievementinfo.hidden == 1 && f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0 && f3.behavior.tools.getachievementshowsecret() == false)
                {
                    value = 'Secret achievement';
                }
                return value;
            },

            'onAfterSetValue': function(object, value, data)
            {
                $(object).removeClass('hide');
            }
        }
    },

    'achievementinfo.strings.unachieved':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                // if it's a secret achievement and no profile unlocked the achievement and the checkbox for showing secret achievements is not set, hide it
                if (data.achievementinfo.hidden == 1 && f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0 && f3.behavior.tools.getachievementshowsecret() == false)
                {
                    value = 'Continue playing to unlock this achievement...';
                }
                //if 'unachieved' is blank (some secret achievements have this, like in Borderlands 2), show the description instead
                return (value !== '') ? value : data.achievementinfo.strings.description;
            },

            'onAfterSetValue': function(object, value, data)
            {
                // only show if nobody unlocked it
                if (f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0) $(object).removeClass('hide');
            }
        }
    },

    'achievementinfo.strings.description':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                // if it's a secret achievement and no profile unlocked the achievement and the checkbox for showing secret achievements is not set, hide it
                if (data.achievementinfo.hidden == 1 && f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0 && f3.behavior.tools.getachievementshowsecret() == false)
                {
                    value = 'Continue playing to unlock this achievement...';
                }
                return value;
            },

            'onAfterSetValue': function(object, value, data)
            {
                // only show if somebody unlocked it
                if (f3.behavior.tools.getachievementunlockcount(data.achievementinfo) > 0) $(object).removeClass('hide');
            }
        }
    },

    'achievementinfo.type':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                if (data.achievementinfo.hidden == 1 && f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0 && f3.behavior.tools.getachievementshowsecret() == false)
                {
                    return '?';
                }
                return f3.common.defines.achievementtypes[value];
            },

            'onAfterSetValue': function(object, value, data)
            {
                $(object).removeClass('hide');
            }
        }
    },

    'achievementinfo.cred':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                if (data.achievementinfo.hidden == 1 && f3.behavior.tools.getachievementunlockcount(data.achievementinfo) == 0 && f3.behavior.tools.getachievementshowsecret() == false)
                {
                    return '- -';
                }
                return value;
            },

            'onAfterSetValue': function(object, value, data)
            {
                $(object).removeClass('hide');
            }
        }
    },

    'achievementinfo.player':
    {
        'image':
        {
            'onBeforeSetValue': function(object, value)
            {
                var index = (typeof $(object).data('index') !== 'undefined') ? $(object).data('index') : -1;
                var image = value[index] == 1 ? 'images/yes.png' : 'images/no.png';
                $(object).attr('data-image', image).css('opacity', value[index] == 1 ? 1 : 0.4);
                return image;
            },

//            'onAfterSetValue': function(object, value)
//            {
//                $(object).parents('tr.f3.hide').removeClass('hide');
//            },

            'onLoadSucceeded': function(object, value)
            {
                //reference is lost in context parsing, must reset the src
                $('[data-image="' + value + '"]').attr('src', value).removeAttr('data-image');  
            }
        }
    },

    'userdata.settings.achievements.showsecret':
    {
        'attribute':
        {
            'onBeforeSetValue': function(object, value, data)
            {
                return (value == true) ? 'checked' : null;
            },

            'onBeforeRefreshOnce': function(object, value)
            {
                //update the settings
                // $.extend(true, f3.data.userdata.settings, { achievements: { showsecret: false } })
                f3.data.userdata.load(
                {
                    complete: function()
                    {
                        f3.data.userdata.settings = { achievements: { showsecret: $(object).prop('checked') } };
                        f3.data.userdata.save(
                        {
                            complete: function()
                            {

                                return value;
                            }
                        });
                    }
                });
                return 0;
            }
        }
    },

    'message':
    {
        'message':
        {
            'onAfterHide': function(object, value)
            {
                $(object).find('span').html('');
            }
        }
    },

    'temperatureinfo.celsius':
    {
        'text':
        {
            'onBeforeSetValue': function(object, value)
            {
                f3.behavior.graphs.temperatureinfo.switchunit(value);
                return (typeof value === 'undefined' || value == true) ? 'C' : 'F';
            }
        }
    },

    'temperatureinfo':
    {
        'text':
        {
            'onAfterRefreshToggle': function(object, value)
            {
                $(object).fadeTo(200, ($(object).data('refresh-current') === 'on') ? 1 : 0.4);
            }
        },


        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var data =
                {
                    "CPU": parseFloat(value['cpu']),
                    "GPU": parseFloat(value['gpu']),
                    "Memory": parseFloat(value['memory']),
                    "Case": parseFloat(value['case'])
                };

                var t = f3.behavior.graphs.temperatureinfo;
                if (t.filled === false)
                {
                    for (var i = 0; i <= t.maxdata; i++)
                    {
                        t.graph.series.addData(data);
                    }
                    t.filled = true;
                }
                else
                {
                    t.graph.series.addData(data);
                }

                return $(object).html();
            },

            'onAfterSetValue': function(object, value)
            {
                f3.behavior.graphs.temperatureinfo.graph.update()
            }
        }
    },


    'memoryinfo':
    {
        'text':
        {
            'onAfterRefreshToggle': function(object, value)
            {
                $(object).fadeTo(200, ($(object).data('refresh-current') === 'on') ? 1 : 0.4);
            }
        },


        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var data =
                {
                    "Used": parseInt(value['used']),
                    "Free": parseInt(value['free'])
                };

                var m = f3.behavior.graphs.memoryinfo;
                if (m.filled === false)
                {
                    for (var i = 0; i <= m.maxdata; i++)
                    {
                        m.graph.series.addData(data);
                    }
                    m.filled = true;
                }
                else
                {
                    m.graph.series.addData(data);
                }

                return $(object).html();
            },

            'onAfterSetValue': function(object, value)
            {
                f3.behavior.graphs.memoryinfo.graph.update()
            }
        }
    },

    'systemlinkbandwidth.bytes.upstream':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.bytestoreadable(value);
            }
        }
    },
    
    'systemlinkbandwidth.bytes.downstream':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                return f3.behavior.tools.bytestoreadable(value);
            }
        }
    },
    
    'systemlinkbandwidth.rate.upstream':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                f3.behavior.graphs.systemlinkbandwidth.maxupstream = Math.max(f3.behavior.graphs.systemlinkbandwidth.maxupstream, value);
                return f3.behavior.tools.bytestoreadable(value) + '/s<br />(max: ' + f3.behavior.tools.bytestoreadable(f3.behavior.graphs.systemlinkbandwidth.maxupstream) + '/s)'
            }
        }
    },
    
    'systemlinkbandwidth.rate.downstream':
    {
        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                f3.behavior.graphs.systemlinkbandwidth.maxdownstream = Math.max(f3.behavior.graphs.systemlinkbandwidth.maxdownstream, value);
                return f3.behavior.tools.bytestoreadable(value) + '/s<br />(max: ' + f3.behavior.tools.bytestoreadable(f3.behavior.graphs.systemlinkbandwidth.maxdownstream) + '/s)'
            }
        }
    },
    
    'systemlinkbandwidth':
    {
        'text':
        {
            'onAfterRefreshToggle': function(object, value)
            {
                $(object).fadeTo(200, ($(object).data('refresh-current') === 'on') ? 1 : 0.4);
            }
        },


        'html':
        {
            'onBeforeSetValue': function(object, value)
            {
                var data =
                {
                    "Upstream": parseFloat(value.rate.upstream),
                    "Downstream": parseFloat(value.rate.downstream),
                };

                var t = f3.behavior.graphs.systemlinkbandwidth;
                if (t.filled === false)
                {
                    for (var i = 0; i <= t.maxdata; i++)
                    {
                        t.graph.series.addData(data);
                    }
                    t.filled = true;
                }
                else
                {
                    t.graph.series.addData(data);
                }

                return $(object).html();
            },

            'onAfterSetValue': function(object, value)
            {
                f3.behavior.graphs.systemlinkbandwidth.graph.update()
            }
        }
    }

};

f3.extend('behavior').conditions =
{
    'liveinfo.gamecapabilities':
    {
        'showhidegamecapabilities': function(object, value)
		{
			var found = false;
            $.each(value, function(n, v)
            {
                found = found || (typeof v == 'string' && v.length > 0);
            });
            if (found === true)
            {
                $(object).show(500);
            }
            else
            {
                $(object).hide();
            }
		}
    },
    
    'gameinfo':
    {
        'showhidegameinfo': function(object, value)
		{
			var found = false;
            $.each(value, function(n, v)
            {
                found = found || (typeof v == 'string' && v.length > 0);
            });
            if (found === true)
            {
                $(object).show();
            }
            else
            {
                $(object).hide();
            }
		}
    }
};