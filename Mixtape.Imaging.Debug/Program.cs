
using NetVips;

using Image im = Image.NewFromFile("image.jpg");

// put im at position (100, 100) in a 3000 x 3000 pixel image,
// make the other pixels in the image by mirroring im up / down /
// left / right, see
// https://www.libvips.org/API/current/method.Image.embed.html
using Image embed = im.Embed(100, 100, 3000, 3000, extend: Enums.Extend.Mirror);

// multiply the green (middle) band by 2, leave the other two alone
using Image multiply = embed * [1, 2, 1];

// make an image from an array constant, convolve with it
using Image mask = Image.NewFromArray(new[,]
{
  {-1, -1, -1},
  {-1, 16, -1},
  {-1, -1, -1}
}, 8);
using Image convolve = multiply.Conv(mask, precision: Enums.Precision.Integer);

// finally, write the result back to a file on disk
convolve.WriteToFile("output.jpg");