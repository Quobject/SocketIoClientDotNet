module.exports = function (grunt) {

  grunt.registerTask('buildClient',
      'build cs modules', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      configuration = grunt.config('msbuild_configuration'),
      output_path_base = 'bin\\'+ configuration +'\\',
      nuget_builds = grunt.config('nuget_builds'),
      tasks = [],
      clean_format = os === 'win' ? '{0} start-process ' +
        '-NoNewWindow ' +
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2} /t:clean;Rebuild  /p:Configuration={3} /p:OutputPath={4} \' ' :
        '{0} {1} /t:Rebuild /p:Configuration={2} ',
      //build_format = os === 'win' ? '{0} start-process ' +
      //  '-NoNewWindow ' +
      //  //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
      //  '-FilePath {1} ' +
      //  '-ArgumentList \' {2} /p:Configuration={3} \' ' :
      //  '{0} {1} /p:Configuration={2}',
      i;

    function addBuildWithTitle(title, dir, copyOnly) {
      var   
        dir_path = string.format('{0}/../../Src/{1}/', __dirname, title),
        csproj = string.format('{0}{1}.csproj', dir_path, title),
        output_path = output_path_base + dir +'\\',
        clean = os === 'win' ? string.format(clean_format, config.win.powershell, config.win.msbuild, csproj, configuration, output_path) :
          string.format(clean_format, config.linux.msbuild, csproj, configuration),
        //build = os === 'win' ? string.format(build_format, config.win.powershell, config.win.msbuild, csproj, configuration ):
        //  string.format(build_format, config.linux.msbuild, csproj, configuration),
        template_file_content = fs.readFileSync('./templates/AssemblyInfo.cs');

      //template_file_content = S(template_file_content).replaceAll('@TITLE@', title).s;
      template_file_content = S(template_file_content).replaceAll('@VERSION@', config.version).s;
      //grunt.log.writeln('template_file_content = "%s"', template_file_content);
      fs.writeFileSync(string.format('{0}Properties/AssemblyInfo.cs', dir_path), template_file_content);
      if (!copyOnly) {
        tasks.push(clean);
        //tasks.push(build);    
      }
    }

    for (i = 0; i < nuget_builds.length; i++) {
      if (nuget_builds[i].Name !== 'SocketIoClientDotNet.netstandard1.3') {
        addBuildWithTitle(nuget_builds[i].Name, nuget_builds[i].NuGetDir, nuget_builds[i].copyOnly);
      }
    }      

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

    if (configuration === 'Release') {
      grunt.task.run('clean:release');
      if (os === 'win') {
        grunt.task.run('copy:release');      
      } else {
        grunt.task.run('copy:release_mono');           
      }
    }
  });
};


