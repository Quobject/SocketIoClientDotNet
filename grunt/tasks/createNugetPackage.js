module.exports = function (grunt) {

  grunt.registerTask('createNugetPackage',
     'create package ', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      working_path = grunt.config('working_path'),
      package_path = working_path + '/NuGet/',
      package_lib_path = working_path + '/NuGet/lib/',
      //configuration = grunt.config('msbuild_configuration'),
      configuration = grunt.config('msbuild_configuration'),
      output_path_base = 'bin\\' + configuration + '\\',
      nuget_builds = grunt.config('nuget_builds'),
      nuget_path = os === 'win' ?
        config.win.nuget : config.linux.nuget,
      dst_path,
      template_file_content,
      i,
      tasks = [];

    //function createPackageWithTitle(title) {
    //  var
    //    dir_path = string.format('{0}/../../{1}/', __dirname, title),
    //    csproj = string.format('{0}{1}.csproj', dir_path, title),
    //    pack = string.format('{0} pack {1}', nuget_path, csproj);

    //  tasks.push(pack);
    //}

    if (os !== 'win') {
      return;
    }
    if (configuration !== 'Release') {
      grunt.log.writeln('wrong configuration = ' + configuration);
      return;
    }

    //createPackageWithTitle('SocketIoClientDotNet');


    if (! fs.existsSync(working_path)) {
      fs.mkdirSync(working_path);
      fs.mkdirSync(package_path);
      fs.mkdirSync(package_lib_path);
    }
    if (!fs.existsSync(package_path)) {
      fs.mkdirSync(package_path);
      fs.mkdirSync(package_lib_path);
    }
    if (!fs.existsSync(package_lib_path)) {
      fs.mkdirSync(package_lib_path);
    }

    for (i = 0; i < nuget_builds.length; i++) {
      dst_path = package_lib_path + nuget_builds[i].NuGetDir + '/';
      //files = fs.readdirSync(dst_path);
      grunt.log.writeln(string.format('dst_path={0}', dst_path));
      fs.mkdirSync(dst_path);
    }
    

    template_file_content = fs.readFileSync('./templates/SocketIoClientDotNet.nuspec');
    template_file_content = S(template_file_content).replaceAll('@VERSION@', config.version).s;
    fs.writeFileSync(string.format('{0}SocketIoClientDotNet.nuspec', package_path), template_file_content);



    function addBuildWithTitle(title, destsubdir, srcsubdir) {
      var
        src_path = string.format('{0}/../../Src/{1}/{2}{3}/', __dirname, title, output_path_base, srcsubdir),
        dst_path = package_lib_path + destsubdir + '/',
        //src_file = string.format('{0}SocketIoClientDotNet.dll', src_path),
        src_file = string.format('{0}SocketIoClientDotNet.dll', src_path),
        dst_file = string.format('{0}SocketIoClientDotNet.dll', dst_path);
      
      grunt.log.writeln(string.format('src_file={0} dst_file={1}', src_file, dst_file));
      fs.writeFileSync(dst_file, fs.readFileSync(src_file));

      //src_file = src_path + string.format('{0}.xml', title);
      //dst_file = string.format('{0}SocketIoClientDotNet.xml', dst_path);
      //grunt.log.writeln(string.format('src_file={0} dst_file={1}', src_file, dst_file));
      //fs.writeFileSync(dst_file, fs.readFileSync(src_file));
    }

    for (i = 0; i < nuget_builds.length; i++) {
      addBuildWithTitle(nuget_builds[i].Name, nuget_builds[i].NuGetDir, nuget_builds[i].SourceDir);
    }
    tasks.push('C:/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe pwd');
    tasks.push(string.format('{0} pack SocketIoClientDotNet.nuspec', config.win.nuget));
    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.options.execOptions.cwd', package_path);
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');       
  });
};