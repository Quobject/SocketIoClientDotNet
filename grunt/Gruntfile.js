
module.exports = function (grunt) {
  var
    node_os = require('os'),
    config = require('./config.json'),
    util = require('util'),
    os = node_os.platform() === 'win32' ? 'win' : 'linux',
    nuget_builds = [
      { "Name": "SocketIoClientDotNet.net35", "NuGetDir": "net35", "SourceDir": "net35", copyOnly: true },
      { "Name": "SocketIoClientDotNet.net40", "NuGetDir": "net40", "SourceDir": "net40", copyOnly: true },
      { "Name": "SocketIoClientDotNet.net45", "NuGetDir": "net45", "SourceDir": "net45", copyOnly: true },
      { "Name": "SocketIoClientDotNet.netstandard1.3", "NuGetDir": "netstandard1.3", "SourceDir": "netstandard1.3", "SourceFileName": "SocketIoClientDotNet.netstandard1.3.dll", copyOnly: true },
    ];

  grunt.log.writeln(util.inspect(config));
  grunt.log.writeln( 'os = "%s"', os );

  grunt.loadTasks('./tasks');

  grunt.initConfig({      
    os: os,
    config: config,
    //msbuild_configuration: 'Debug',
    msbuild_configuration: 'Release',
    nuget_builds: nuget_builds,
    release_path: './../Releases/<%= config.version %>/',
    working_path: './../Working/',
    server_path: '../TestServer/',
    shell: {
      exec: {
        options: {
          stdout: true,
          stderr: true
        }
      }
    },
    jshint: {
      options: {
        jshintrc: true,
      },
      target: ['Gruntfile.js', '<%= server_path %>server.js', 'tasks/**/*.js']
    },
    clean: {
      release: ['<%= release_path %>/*'],
      working: ['<%= working_path %>/*'],
      options: { force: true }                
    },  
    copy: {
      release: {
        files: [
          {
            expand: true,
            cwd:  './../SocketIoClientDotNet/bin/Release',
            src:  '*',
            dest: '<%= release_path %>/net45'
          }
        ]
      },
      release_mono: {
        files: [
          {
            expand: true,
            cwd:  './../SocketIoClientDotNet_Mono/bin/Release',
            src:  '*',
            dest: '<%= release_path %>/mono'
          }
        ]
      },
    }
  });

  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.loadNpmTasks('grunt-contrib-clean');  
  grunt.loadNpmTasks('grunt-shell');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.registerTask('default', ['jshint', 'installNpm', 'nuget', 'buildClient', 'buildTest', 'startServer', 'testClient']);
  grunt.registerTask('test', ['jshint', 'buildClient', 'buildTest', 'testClient']);
  grunt.registerTask('makeNuget', ['jshint','clean:working','createNugetPackage']);
  grunt.registerTask('makeComponent', ['jshint','clean:working','createNugetPackage','createXamarinComponent']);
};
